using System.Net;
using Aduo.Core.Infrastructure.Persistence;
using Aduo.Core.Modules.Identity.Auth;
using Aduo.Core.Modules.Identity.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Aduo.Api.IntegrationTests;

/// <summary>
/// ID-T12..14, T45. Phần Google gọi lại được thử tay trên trình duyệt (cần tài khoản Google thật);
/// ở đây kiểm tra logic nghiệp vụ sau khi Google đã xác nhận danh tính, và các endpoint không cần Google.
/// </summary>
[Collection(ApiCollection.Name)]
public class GoogleLoginTests(ApiFactory factory)
{
    private async Task<ExternalLoginResult> LoginAsync(ExternalIdentity ext)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<ExternalLoginHandler>()
            .HandleAsync(ext, ClientInfo.Unknown, CancellationToken.None);
    }

    private static ExternalIdentity NewGoogleUser(string? email = null, bool verified = true)
        => new("Google", Guid.NewGuid().ToString("N"), email ?? TestUsers.NewEmail(), verified, "Minh Google");

    [Fact]
    public async Task NewGoogleUser_IsCreatedVerifiedWithSession() // ID-T12
    {
        var ext = NewGoogleUser();

        var result = await LoginAsync(ext);

        Assert.Equal(ExternalLoginOutcome.SignedUpNew, result.Outcome);
        Assert.NotNull(result.Tokens);
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = await db.Users.SingleAsync(u => u.Email == ext.Email);
        Assert.True(user.EmailConfirmed);
        Assert.Null(user.PasswordHash);
        Assert.Equal(1, await db.UserConsents.CountAsync(c => c.UserId == user.Id));
    }

    [Fact]
    public async Task ReturningGoogleUser_SignsInWithoutDuplicate() // ID-T12 (lần sau)
    {
        var ext = NewGoogleUser();
        await LoginAsync(ext);

        var again = await LoginAsync(ext);

        Assert.Equal(ExternalLoginOutcome.SignedIn, again.Outcome);
        await using var scope = factory.Services.CreateAsyncScope();
        Assert.Equal(1, await scope.ServiceProvider.GetRequiredService<AppDbContext>().Users.CountAsync(u => u.Email == ext.Email));
    }

    [Fact]
    public async Task GoogleEmailMatchingPasswordAccount_IsNotAutoLinked() // ID-T13
    {
        var (_, email) = await TestUsers.SignedInAsync(factory);

        var result = await LoginAsync(NewGoogleUser(email));

        Assert.Equal(ExternalLoginOutcome.EmailExistsNeedsPassword, result.Outcome);
        Assert.Null(result.Tokens);
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var existingId = await db.Users.Where(u => u.Email == email).Select(u => u.Id).SingleAsync();
        Assert.Equal(0, await db.Set<Microsoft.AspNetCore.Identity.IdentityUserLogin<Guid>>().CountAsync(l => l.UserId == existingId));
    }

    [Fact]
    public async Task UnverifiedGoogleEmail_IsRejected()
    {
        var result = await LoginAsync(NewGoogleUser(verified: false));

        Assert.Equal(ExternalLoginOutcome.EmailNotVerified, result.Outcome);
    }

    [Theory] // ID-T14
    [InlineData("https://evil.com", "/")]
    [InlineData("//evil.com", "/")]
    [InlineData("/\\evil.com", "/")]
    [InlineData("javascript:alert(1)", "/")]
    [InlineData("/hoc/bai-1", "/hoc/bai-1")]
    [InlineData(null, "/")]
    public void ReturnUrl_OnlyInternalPathsAreAccepted(string? input, string expected)
        => Assert.Equal(expected, IdentityRedirects.SafeReturnUrl(input));

    [Fact]
    public async Task GoogleLogin_StoresNoGoogleTokens() // ID-T45
    {
        var ext = NewGoogleUser();
        await LoginAsync(ext);

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userId = await db.Users.Where(u => u.Email == ext.Email).Select(u => u.Id).SingleAsync();
        Assert.Equal(0, await db.Set<Microsoft.AspNetCore.Identity.IdentityUserToken<Guid>>().CountAsync(t => t.UserId == userId));
        Assert.Equal(1, await db.RefreshTokens.CountAsync(t => t.UserId == userId)); // phiên ADUO giống đăng nhập thường
    }

    [Fact]
    public async Task Providers_ReportsGoogleDisabledWhenNotConfigured_AndStartReturns404()
    {
        var client = new TestClient(factory);

        var providers = await TestClient.JsonAsync(await client.GetAsync("/api/v1/auth/external/providers"));
        var start = await client.GetAsync("/api/v1/auth/external/google");

        Assert.False(providers.GetProperty("google").GetBoolean());
        Assert.Equal(HttpStatusCode.NotFound, start.StatusCode);
    }
}
