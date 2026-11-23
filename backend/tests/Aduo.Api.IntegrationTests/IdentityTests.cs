using System.Net;
using System.Text.RegularExpressions;
using Aduo.Core.Common.Security;
using Aduo.Core.Infrastructure.Persistence;
using Aduo.Core.Modules.Identity.Domain;
using Aduo.Core.Modules.Identity.Jobs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Aduo.Api.IntegrationTests;

/// <summary>Test case ID-T01..ID-T45 của docs/phase-4/modules/01-identity.md. Cần Docker (PostgreSQL thật).</summary>
[Collection(ApiCollection.Name)]
public class IdentityTests(ApiFactory factory)
{
    private const string Auth = "/api/v1/auth";

    // ───────────── Đăng ký, xác minh, đăng nhập ─────────────

    [Fact]
    public async Task Register_Valid_CreatesUserConsentCookiesAndEmail() // ID-T01
    {
        var client = new TestClient(factory);
        await client.EnsureCsrfAsync();
        var email = TestUsers.NewEmail();

        var res = await client.PostAsync($"{Auth}/register", TestUsers.Register(email));

        Assert.Equal(HttpStatusCode.Created, res.StatusCode);
        var me = await TestClient.JsonAsync(res);
        Assert.False(me.GetProperty("emailConfirmed").GetBoolean());
        Assert.Contains(client.LastSetCookies, c => c.StartsWith("aduo_at=") && c.Contains("httponly", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(client.LastSetCookies, c => c.StartsWith("aduo_rt=") && c.Contains("path=/api/v1/auth", StringComparison.OrdinalIgnoreCase));
        Assert.NotNull(factory.Emails.LastTo(email));

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = await db.Users.SingleAsync(u => u.Email == email);
        Assert.Equal(1, await db.UserConsents.CountAsync(c => c.UserId == user.Id));
    }

    [Fact]
    public async Task Register_DuplicateEmailDifferentCase_Returns409() // ID-T02
    {
        var (client, email) = await TestUsers.SignedInAsync(factory);
        var other = new TestClient(factory);
        await other.EnsureCsrfAsync();

        var res = await other.PostAsync($"{Auth}/register", TestUsers.Register(email.ToUpperInvariant()));

        Assert.Equal(HttpStatusCode.Conflict, res.StatusCode);
        Assert.Equal("EMAIL_TAKEN", await TestClient.CodeAsync(res));
        _ = client;
    }

    [Fact]
    public async Task Register_WeakPassword_Returns400OnPasswordField() // ID-T03
    {
        var client = new TestClient(factory);
        await client.EnsureCsrfAsync();

        var res = await client.PostAsync($"{Auth}/register", TestUsers.Register(TestUsers.NewEmail(), "12345678"));

        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
        var json = await TestClient.JsonAsync(res);
        Assert.True(json.GetProperty("errors").TryGetProperty("password", out _));
    }

    [Fact]
    public async Task Register_WithoutAcceptingTerms_Returns400() // ID-T04
    {
        var client = new TestClient(factory);
        await client.EnsureCsrfAsync();
        var body = new { displayName = "Lan", email = TestUsers.NewEmail(), password = TestUsers.Password, acceptTerms = false };

        var res = await client.PostAsync($"{Auth}/register", body);

        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }

    [Fact]
    public async Task Register_WithoutXsrfHeader_Returns400CsrfInvalid() // ID-T05
    {
        var client = new TestClient(factory);
        await client.EnsureCsrfAsync();

        var res = await client.PostAsync($"{Auth}/register", TestUsers.Register(TestUsers.NewEmail()), csrf: false);

        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
        Assert.Equal("CSRF_INVALID", await TestClient.CodeAsync(res));
    }

    [Fact]
    public async Task ConfirmEmail_ValidToken_MarksEmailConfirmed() // ID-T06
    {
        var (client, email) = await TestUsers.SignedInAsync(factory);
        var (userId, token) = ParseLink(factory.Emails.LastTo(email)!.TextBody, "userId");

        var res = await client.PostAsync($"{Auth}/confirm-email", new { userId, token });

        Assert.Equal(HttpStatusCode.NoContent, res.StatusCode);
        var me = await TestClient.JsonAsync(await client.GetAsync("/api/v1/me"));
        Assert.True(me.GetProperty("emailConfirmed").GetBoolean());
        Assert.Contains("assessment.take", me.GetProperty("permissions").EnumerateArray().Select(p => p.GetString()));
    }

    [Fact]
    public async Task ConfirmEmail_After25Hours_Returns400TokenExpired() // ID-T07
    {
        var (client, email) = await TestUsers.SignedInAsync(factory);
        var (userId, token) = ParseLink(factory.Emails.LastTo(email)!.TextBody, "userId");

        factory.Time.Advance(TimeSpan.FromHours(25));
        var res = await client.PostAsync($"{Auth}/confirm-email", new { userId, token });

        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
        Assert.Equal("TOKEN_EXPIRED", await TestClient.CodeAsync(res));
    }

    [Fact]
    public async Task Me_UnverifiedEmail_LacksVerifiedOnlyPermissions() // ID-T17 (endpoint assessment.take chưa có, kiểm tra quyền trong /me)
    {
        var (client, _) = await TestUsers.SignedInAsync(factory);

        var me = await TestClient.JsonAsync(await client.GetAsync("/api/v1/me"));
        var perms = me.GetProperty("permissions").EnumerateArray().Select(p => p.GetString()).ToList();

        Assert.Contains("learning.access", perms);
        Assert.DoesNotContain("assessment.take", perms);
        Assert.DoesNotContain("reports.create", perms);
    }

    [Fact]
    public async Task Login_WrongPasswordAndUnknownEmail_ReturnSameError() // ID-T08
    {
        var (_, email) = await TestUsers.SignedInAsync(factory);
        var client = new TestClient(factory);
        await client.EnsureCsrfAsync();

        var wrong = await client.PostAsync($"{Auth}/login", new { email, password = "Sai-mat-khau-1" });
        var unknown = await client.PostAsync($"{Auth}/login", new { email = TestUsers.NewEmail(), password = "Sai-mat-khau-1" });

        Assert.Equal(HttpStatusCode.Unauthorized, wrong.StatusCode);
        Assert.Equal(wrong.StatusCode, unknown.StatusCode);
        Assert.Equal("INVALID_CREDENTIALS", await TestClient.CodeAsync(wrong));
        Assert.Equal("INVALID_CREDENTIALS", await TestClient.CodeAsync(unknown));
    }

    [Fact]
    public async Task Login_FiveWrongAttempts_LocksAccountUntilLockoutEnds() // ID-T09
    {
        var (_, email) = await TestUsers.SignedInAsync(factory);
        var client = new TestClient(factory);
        await client.EnsureCsrfAsync();

        for (var i = 0; i < 5; i++)
        {
            await client.PostAsync($"{Auth}/login", new { email, password = "Sai-mat-khau-1" });
        }

        var locked = await client.PostAsync($"{Auth}/login", new { email, password = TestUsers.Password });
        Assert.Equal("ACCOUNT_LOCKED", await TestClient.CodeAsync(locked));

        // Hết 15 phút: UserManager dùng đồng hồ hệ thống nên đẩy LockoutEnd về quá khứ trực tiếp trong DB.
        await ExecAsync(db => db.Users.Where(u => u.Email == email)
            .ExecuteUpdateAsync(s => s.SetProperty(u => u.LockoutEnd, DateTimeOffset.UtcNow.AddMinutes(-1))));
        var ok = await client.PostAsync($"{Auth}/login", new { email, password = TestUsers.Password });
        Assert.Equal(HttpStatusCode.OK, ok.StatusCode);
    }

    [Fact]
    public async Task ForgotPassword_UnknownEmail_Returns202AndSendsNothing() // ID-T10
    {
        var client = new TestClient(factory);
        await client.EnsureCsrfAsync();
        var email = TestUsers.NewEmail();

        var res = await client.PostAsync($"{Auth}/forgot-password", new { email });

        Assert.Equal(HttpStatusCode.Accepted, res.StatusCode);
        Assert.Null(factory.Emails.LastTo(email));
    }

    [Fact]
    public async Task ResetPassword_WorksOnce_AndRevokesOldSessions() // ID-T11 + ID-T34
    {
        var (client, email) = await TestUsers.SignedInAsync(factory);
        var oldAccess = client.Cookie("aduo_at")!;
        var oldRefresh = client.Cookie("aduo_rt")!;

        await client.PostAsync($"{Auth}/forgot-password", new { email });
        var (_, token) = ParseLink(factory.Emails.LastTo(email)!.TextBody, "email");

        var first = await client.PostAsync($"{Auth}/reset-password", new { email, token, newPassword = "MatKhauMoi99" });
        var second = await client.PostAsync($"{Auth}/reset-password", new { email, token, newPassword = "MatKhauKhac99" });

        Assert.Equal(HttpStatusCode.NoContent, first.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, second.StatusCode); // token chết sau khi đổi SecurityStamp

        var other = new TestClient(factory);
        other.SetCookie("aduo_rt", oldRefresh, "/api/v1/auth");
        await other.EnsureCsrfAsync();
        Assert.Equal(HttpStatusCode.Unauthorized, (await other.PostAsync($"{Auth}/refresh")).StatusCode);

        var stale = await new TestClient(factory).SendAsync(HttpMethod.Get, "/api/v1/me", bearer: oldAccess);
        Assert.Equal(HttpStatusCode.Unauthorized, stale.StatusCode);

        var login = await other.PostAsync($"{Auth}/login", new { email, password = "MatKhauMoi99" });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
    }

    [Fact]
    public async Task PatchMe_IgnoresEmail() // ID-T15
    {
        var (client, email) = await TestUsers.SignedInAsync(factory);

        var res = await client.SendAsync(HttpMethod.Patch, "/api/v1/me", new { displayName = "Lan Anh", email = "hacker@example.com" });

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        var me = await TestClient.JsonAsync(res);
        Assert.Equal(email, me.GetProperty("email").GetString());
        Assert.Equal("Lan Anh", me.GetProperty("displayName").GetString());
    }

    [Fact]
    public async Task AdminEndpoints_AsStudent_Return403() // ID-T16
    {
        var (client, _) = await TestUsers.SignedInAsync(factory);

        var res = await client.GetAsync("/api/v1/admin/users");

        Assert.Equal(HttpStatusCode.Forbidden, res.StatusCode);
    }

    [Fact]
    public async Task DeleteAccount_BlocksLogin_AndJobAnonymizesEmail() // ID-T18
    {
        var (client, email) = await TestUsers.SignedInAsync(factory);

        var del = await client.PostAsync("/api/v1/me/delete", new { password = TestUsers.Password, confirmText = "XÓA" });
        Assert.Equal(HttpStatusCode.NoContent, del.StatusCode);

        var login = await client.PostAsync($"{Auth}/login", new { email, password = TestUsers.Password });
        Assert.Equal(HttpStatusCode.Unauthorized, login.StatusCode);

        factory.Time.Advance(TimeSpan.FromDays(8));
        await using var scope = factory.Services.CreateAsyncScope();
        await ActivatorUtilities.CreateInstance<PurgeDeletedAccountsJob>(scope.ServiceProvider,
            scope.ServiceProvider.GetServices<Aduo.Core.Common.Privacy.IUserDataEraser>()).RunAsync(CancellationToken.None);

        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.False(await db.Users.AnyAsync(u => u.Email == email));
    }

    [Fact]
    public async Task AdminLock_TakesEffectImmediately_AndLastAdminIsProtected() // ID-T19, ID-T20, ID-T37
    {
        var (admin, adminEmail) = await CreateAdminAsync();
        var (student, studentEmail) = await TestUsers.SignedInAsync(factory);
        var studentId = await UserIdAsync(studentEmail);

        var locked = await admin.PostAsync($"/api/v1/admin/users/{studentId}/lock");
        Assert.Equal(HttpStatusCode.NoContent, locked.StatusCode);

        var after = await student.GetAsync("/api/v1/me");
        Assert.Equal(HttpStatusCode.Unauthorized, after.StatusCode);

        // Quản trị viên cuối cùng không bị khóa. Các test khác có thể tạo thêm admin, nên khóa hết admin khác trước.
        var adminId = await UserIdAsync(adminEmail);
        await ExecAsync(async db =>
        {
            var adminRoleId = await db.Roles.Where(r => r.Name == Roles.Admin).Select(r => r.Id).SingleAsync();
            var others = await db.UserRoles.Where(ur => ur.RoleId == adminRoleId && ur.UserId != adminId).Select(ur => ur.UserId).ToListAsync();
            await db.Users.Where(u => others.Contains(u.Id)).ExecuteUpdateAsync(s => s.SetProperty(u => u.LockoutEnd, DateTimeOffset.MaxValue));
        });
        var last = await admin.PostAsync($"/api/v1/admin/users/{adminId}/lock");
        Assert.Equal(HttpStatusCode.UnprocessableEntity, last.StatusCode);
        Assert.Equal("LAST_ADMIN", await TestClient.CodeAsync(last));
    }

    // ───────────── JWT hybrid (AD-09) ─────────────

    [Fact]
    public async Task Login_SetsCookiesWithRequiredAttributes_AndNoTokenInBody() // ID-T24
    {
        var (_, email) = await TestUsers.SignedInAsync(factory);
        var client = new TestClient(factory);
        await client.EnsureCsrfAsync();

        var res = await client.PostAsync($"{Auth}/login", new { email, password = TestUsers.Password });

        var body = await res.Content.ReadAsStringAsync();
        Assert.DoesNotContain(client.Cookie("aduo_at")!, body);
        Assert.DoesNotContain(client.Cookie("aduo_rt")!, body);
        var access = client.LastSetCookies.Single(c => c.StartsWith("aduo_at="));
        var refresh = client.LastSetCookies.Single(c => c.StartsWith("aduo_rt="));
        Assert.Contains("httponly", access, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("samesite=lax", access, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("httponly", refresh, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("path=/api/v1/auth", refresh, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("expires=", refresh, StringComparison.OrdinalIgnoreCase); // không "Ghi nhớ": cookie phiên
    }

    [Fact]
    public async Task AccessToken_After10Minutes_Returns401TokenExpired() // ID-T25
    {
        var (client, _) = await TestUsers.SignedInAsync(factory);

        factory.Time.Advance(TimeSpan.FromMinutes(11));
        var res = await client.GetAsync("/api/v1/me");

        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
        Assert.Equal("TOKEN_EXPIRED", await TestClient.CodeAsync(res));
    }

    [Fact]
    public async Task Refresh_Valid_RotatesTokenAndMarksOldAsUsed() // ID-T26
    {
        var (client, _) = await TestUsers.SignedInAsync(factory);
        var oldRefresh = client.Cookie("aduo_rt")!;

        factory.Time.Advance(TimeSpan.FromMinutes(11));
        var res = await client.PostAsync($"{Auth}/refresh");

        Assert.Equal(HttpStatusCode.NoContent, res.StatusCode);
        Assert.NotEqual(oldRefresh, client.Cookie("aduo_rt"));
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/v1/me")).StatusCode);
        var hash = Aduo.Core.Modules.Identity.Auth.TokenHashing.Hash(oldRefresh);
        await ExecAsync(async db => Assert.NotNull((await db.RefreshTokens.SingleAsync(t => t.TokenHash == hash)).UsedAt));
    }

    [Fact]
    public async Task Refresh_ReusingOldTokenAfterGrace_RevokesWholeFamily() // ID-T27
    {
        var (client, _) = await TestUsers.SignedInAsync(factory);
        var stolen = client.Cookie("aduo_rt")!;
        await client.PostAsync($"{Auth}/refresh"); // người dùng thật xoay token

        factory.Time.Advance(TimeSpan.FromSeconds(30)); // quá ân hạn 10 giây
        var thief = new TestClient(factory);
        thief.SetCookie("aduo_rt", stolen, "/api/v1/auth");
        await thief.EnsureCsrfAsync();
        var res = await thief.PostAsync($"{Auth}/refresh");

        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
        Assert.Equal("REFRESH_REUSED", await TestClient.CodeAsync(res));

        // Cả chuỗi bị thu hồi: token mới của người dùng thật cũng chết, access đang dùng cũng bị từ chối.
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.PostAsync($"{Auth}/refresh")).StatusCode);
        client.SetCookie("aduo_rt", "x", "/api/v1/auth");
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/v1/me")).StatusCode);
    }

    [Fact]
    public async Task Refresh_TwoParallelRequestsWithSameToken_BothSucceedWithSingleSuccessor() // ID-T28
    {
        var (client, _) = await TestUsers.SignedInAsync(factory);
        var raw = client.Cookie("aduo_rt")!;
        var a = CloneWith(raw, client);
        var b = CloneWith(raw, client);

        var results = await Task.WhenAll(a.PostAsync($"{Auth}/refresh"), b.PostAsync($"{Auth}/refresh"));

        Assert.All(results, r => Assert.Equal(HttpStatusCode.NoContent, r.StatusCode));
        var oldHash = Aduo.Core.Modules.Identity.Auth.TokenHashing.Hash(raw);
        await ExecAsync(async db =>
        {
            var family = (await db.RefreshTokens.SingleAsync(t => t.TokenHash == oldHash)).FamilyId;
            Assert.Equal(2, await db.RefreshTokens.CountAsync(t => t.FamilyId == family)); // gốc + đúng một kế nhiệm
        });
    }

    [Fact]
    public async Task Refresh_AfterIdleTimeout_Returns401() // ID-T29
    {
        var (client, _) = await TestUsers.SignedInAsync(factory);

        factory.Time.Advance(TimeSpan.FromHours(25));
        var res = await client.PostAsync($"{Auth}/refresh");

        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    [Fact]
    public async Task Refresh_ActiveButPastAbsoluteLifetime_Returns401() // ID-T30
    {
        var email = TestUsers.NewEmail();
        var client = new TestClient(factory);
        await client.EnsureCsrfAsync();
        await client.PostAsync($"{Auth}/register", TestUsers.Register(email));
        await client.PostAsync($"{Auth}/login", new { email, password = TestUsers.Password, rememberMe = true });

        // Dùng đều đặn mỗi 20 ngày (hạn nghỉ 30 ngày) nhưng hạn tuyệt đối 90 ngày vẫn không đổi.
        for (var i = 0; i < 4; i++)
        {
            factory.Time.Advance(TimeSpan.FromDays(20));
            Assert.Equal(HttpStatusCode.NoContent, (await client.PostAsync($"{Auth}/refresh")).StatusCode);
        }

        factory.Time.Advance(TimeSpan.FromDays(20)); // 100 ngày kể từ lúc đăng nhập
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.PostAsync($"{Auth}/refresh")).StatusCode);
    }

    [Fact]
    public async Task Jwt_SignedWithUnknownKey_Returns401() // ID-T31
    {
        var (_, email) = await TestUsers.SignedInAsync(factory);
        var token = ForgeToken(new SymmetricSecurityKey("another-secret-key-0123456789-0123456789"u8.ToArray()), await UserIdAsync(email));

        var res = await new TestClient(factory).SendAsync(HttpMethod.Get, "/api/v1/me", bearer: token);

        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    [Fact]
    public async Task Jwt_AlgNone_Returns401() // ID-T32
    {
        var (_, email) = await TestUsers.SignedInAsync(factory);
        var token = ForgeToken(null, await UserIdAsync(email));

        var res = await new TestClient(factory).SendAsync(HttpMethod.Get, "/api/v1/me", bearer: token);

        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    [Theory] // ID-T33
    [InlineData("evil", "aduo-web")]
    [InlineData("aduo", "evil")]
    public async Task Jwt_WrongIssuerOrAudience_Returns401(string issuer, string audience)
    {
        var (_, email) = await TestUsers.SignedInAsync(factory);
        var key = new SymmetricSecurityKey("test-only-jwt-key-0123456789-0123456789-abcdef"u8.ToArray()) { KeyId = "test-1" };
        var token = ForgeToken(key, await UserIdAsync(email), issuer, audience);

        var res = await new TestClient(factory).SendAsync(HttpMethod.Get, "/api/v1/me", bearer: token);

        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    [Fact]
    public async Task ChangePassword_InvalidatesOtherSessions_KeepsCurrent() // ID-T34
    {
        var (device1, email) = await TestUsers.SignedInAsync(factory);
        var device2 = new TestClient(factory);
        await device2.EnsureCsrfAsync();
        await device2.PostAsync($"{Auth}/login", new { email, password = TestUsers.Password });
        var oldAccess2 = device2.Cookie("aduo_at")!;

        var res = await device1.PostAsync("/api/v1/me/change-password", new { currentPassword = TestUsers.Password, newPassword = "MatKhauMoi99" });

        Assert.Equal(HttpStatusCode.NoContent, res.StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await device1.GetAsync("/api/v1/me")).StatusCode); // phiên hiện tại sống
        var stale = await new TestClient(factory).SendAsync(HttpMethod.Get, "/api/v1/me", bearer: oldAccess2);
        Assert.Equal(HttpStatusCode.Unauthorized, stale.StatusCode);
    }

    [Fact]
    public async Task Logout_RevokesSessionAndClearsCookies() // ID-T35
    {
        var (client, _) = await TestUsers.SignedInAsync(factory);
        var access = client.Cookie("aduo_at")!;
        var refresh = client.Cookie("aduo_rt")!;

        var res = await client.PostAsync($"{Auth}/logout");

        Assert.Equal(HttpStatusCode.NoContent, res.StatusCode);
        Assert.Null(client.Cookie("aduo_at"));
        Assert.Null(client.Cookie("aduo_rt"));

        var again = new TestClient(factory);
        again.SetCookie("aduo_rt", refresh, "/api/v1/auth");
        await again.EnsureCsrfAsync();
        Assert.Equal(HttpStatusCode.Unauthorized, (await again.PostAsync($"{Auth}/refresh")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await new TestClient(factory).SendAsync(HttpMethod.Get, "/api/v1/me", bearer: access)).StatusCode);
    }

    [Fact]
    public async Task LogoutAll_RevokesEverySession() // ID-T36
    {
        var (device1, email) = await TestUsers.SignedInAsync(factory);
        var device2 = new TestClient(factory);
        await device2.EnsureCsrfAsync();
        await device2.PostAsync($"{Auth}/login", new { email, password = TestUsers.Password });
        var refresh2 = device2.Cookie("aduo_rt")!;

        await device1.PostAsync($"{Auth}/logout-all");

        var again = new TestClient(factory);
        again.SetCookie("aduo_rt", refresh2, "/api/v1/auth");
        await again.EnsureCsrfAsync();
        Assert.Equal(HttpStatusCode.Unauthorized, (await again.PostAsync($"{Auth}/refresh")).StatusCode);
    }

    [Fact]
    public async Task AdminSession_OlderThan12Hours_RequiresReauth() // ID-T38
    {
        var (admin, _) = await CreateAdminAsync();
        Assert.Equal(HttpStatusCode.OK, (await admin.GetAsync("/api/v1/admin/users")).StatusCode);

        // Hạn tuyệt đối của phiên Admin cũng là 12 giờ nên bằng thời gian thuần không thể "còn sống mà quá 12 giờ".
        // Ngưỡng auth_time là lớp bảo vệ thứ hai (đổi cấu hình AdminReauthHours được), nên giả lập auth_time cũ trong DB.
        await ExecAsync(db => db.RefreshTokens.ExecuteUpdateAsync(s =>
            s.SetProperty(t => t.AuthTime, t => t.AuthTime.AddHours(-13))));
        Assert.Equal(HttpStatusCode.NoContent, (await admin.PostAsync($"{Auth}/refresh")).StatusCode);

        var res = await admin.GetAsync("/api/v1/admin/users");
        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
        Assert.Equal("REAUTH_REQUIRED", await TestClient.CodeAsync(res));
    }

    [Fact]
    public async Task Sessions_ListMarksCurrent_AndRevokingOtherWorks() // ID-T39
    {
        var (device1, email) = await TestUsers.SignedInAsync(factory);
        var device2 = new TestClient(factory);
        await device2.EnsureCsrfAsync();
        await device2.PostAsync($"{Auth}/login", new { email, password = TestUsers.Password });

        var list = await TestClient.JsonAsync(await device1.GetAsync("/api/v1/me/sessions"));
        Assert.Equal(2, list.GetArrayLength());
        var other = list.EnumerateArray().Single(s => !s.GetProperty("current").GetBoolean());
        Assert.Single(list.EnumerateArray(), s => s.GetProperty("current").GetBoolean());

        var del = await device1.SendAsync(HttpMethod.Delete, $"/api/v1/me/sessions/{other.GetProperty("id").GetString()}");
        Assert.Equal(HttpStatusCode.NoContent, del.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await device2.PostAsync($"{Auth}/refresh")).StatusCode);
    }

    [Fact]
    public async Task Refresh_WithoutXsrfHeader_Returns400() // ID-T41
    {
        var (client, _) = await TestUsers.SignedInAsync(factory);

        var res = await client.PostAsync($"{Auth}/refresh", csrf: false);

        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
        Assert.Equal("CSRF_INVALID", await TestClient.CodeAsync(res));
    }

    [Fact]
    public async Task BearerOnly_DoesNotNeedXsrf() // ID-T42
    {
        var (client, _) = await TestUsers.SignedInAsync(factory);
        var access = client.Cookie("aduo_at")!;

        var native = new TestClient(factory);
        var res = await native.SendAsync(HttpMethod.Patch, "/api/v1/me", new { displayName = "Qua Bearer" }, csrf: false, bearer: access, sendCookies: false);

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
    }

    [Fact]
    public async Task AuthEvents_RecordSecurityEvents_WithoutSecrets() // ID-T43
    {
        var (client, email) = await TestUsers.SignedInAsync(factory);
        var access = client.Cookie("aduo_at")!;
        var refresh = client.Cookie("aduo_rt")!;
        await client.PostAsync($"{Auth}/login", new { email, password = "Sai-mat-khau-1" });
        var stolen = refresh;
        await client.PostAsync($"{Auth}/refresh");
        factory.Time.Advance(TimeSpan.FromSeconds(30));
        var thief = CloneWith(stolen, client);
        await thief.PostAsync($"{Auth}/refresh");

        await ExecAsync(async db =>
        {
            var userId = await db.Users.Where(u => u.Email == email).Select(u => u.Id).SingleAsync();
            var events = await db.AuthEvents.Where(e => e.UserId == userId).ToListAsync();
            Assert.Contains(events, e => e.Kind == AuthEventKind.LoginSucceeded);
            Assert.Contains(events, e => e.Kind == AuthEventKind.LoginFailed);
            Assert.Contains(events, e => e.Kind == AuthEventKind.RefreshReuseDetected);
            var dump = string.Join("|", events.Select(e => e.Details + e.IpPrefix + e.UserAgent));
            Assert.DoesNotContain(TestUsers.Password, dump);
            Assert.DoesNotContain(access, dump);
            Assert.DoesNotContain(refresh, dump);
            Assert.DoesNotContain(email, dump);
        });
    }

    [Fact]
    public async Task RefreshTokens_StoredOnlyAsHash() // ID-T44
    {
        var (client, _) = await TestUsers.SignedInAsync(factory);
        var raw = client.Cookie("aduo_rt")!;

        await ExecAsync(async db =>
        {
            var hash = Aduo.Core.Modules.Identity.Auth.TokenHashing.Hash(raw);
            var row = await db.RefreshTokens.SingleAsync(t => t.TokenHash == hash);
            Assert.Equal(32, row.TokenHash.Length); // SHA-256
            Assert.DoesNotContain(raw, row.DeviceLabel + row.UserAgent + row.IpPrefix);
            Assert.NotEqual(System.Text.Encoding.UTF8.GetBytes(raw), row.TokenHash);
        });
    }

    // ───────────── helpers ─────────────

    private TestClient CloneWith(string refreshToken, TestClient source)
    {
        var c = new TestClient(factory);
        c.SetCookie("aduo_rt", refreshToken, "/api/v1/auth");
        c.SetCookie("XSRF-TOKEN", source.Cookie("XSRF-TOKEN")!);
        if (source.Cookie("aduo_at") is { } at)
        {
            c.SetCookie("aduo_at", at);
        }

        return c;
    }

    private static (string UserId, string Token) ParseLink(string body, string idParam)
    {
        var token = Regex.Match(body, @"token=([A-Za-z0-9_\-]+)").Groups[1].Value;
        var id = Uri.UnescapeDataString(Regex.Match(body, idParam + @"=([^&\s]+)").Groups[1].Value);
        return (id, token);
    }

    private async Task ExecAsync(Func<AppDbContext, Task> action)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        await action(scope.ServiceProvider.GetRequiredService<AppDbContext>());
    }

    private async Task<Guid> UserIdAsync(string email)
    {
        Guid id = default;
        await ExecAsync(async db => id = await db.Users.Where(u => u.Email == email).Select(u => u.Id).SingleAsync());
        return id;
    }

    private async Task<(TestClient Client, string Email)> CreateAdminAsync()
    {
        var email = TestUsers.NewEmail();
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
            var user = new AppUser { UserName = email, Email = email, EmailConfirmed = true, DisplayName = "Quản trị", CreatedAt = DateTimeOffset.UtcNow };
            Assert.True((await users.CreateAsync(user, TestUsers.Password)).Succeeded);
            Assert.True((await users.AddToRoleAsync(user, Roles.Admin)).Succeeded);
        }

        var client = new TestClient(factory);
        await client.EnsureCsrfAsync();
        var login = await client.PostAsync($"{Auth}/login", new { email, password = TestUsers.Password });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        return (client, email);
    }

    private string ForgeToken(SecurityKey? key, Guid userId, string issuer = "aduo", string audience = "aduo-web")
    {
        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = issuer,
            Audience = audience,
            Expires = factory.Time.GetUtcNow().UtcDateTime.AddMinutes(5),
            NotBefore = factory.Time.GetUtcNow().UtcDateTime.AddMinutes(-1),
            SigningCredentials = key is null ? null : new SigningCredentials(key, SecurityAlgorithms.HmacSha256),
            Claims = new Dictionary<string, object>
            {
                ["sub"] = userId.ToString(),
                ["sid"] = Guid.NewGuid().ToString(),
                ["sst"] = "x",
                ["auth_time"] = factory.Time.GetUtcNow().ToUnixTimeSeconds(),
            },
        };
        return new JsonWebTokenHandler().CreateToken(descriptor);
    }
}
