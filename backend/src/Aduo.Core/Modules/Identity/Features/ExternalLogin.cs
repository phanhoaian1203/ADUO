using Aduo.Core.Common.Security;
using Aduo.Core.Infrastructure.Persistence;
using Aduo.Core.Modules.Identity.Auth;
using Aduo.Core.Modules.Identity.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Aduo.Core.Modules.Identity.Features;

/// <summary>Thông tin người dùng do nhà cung cấp đăng nhập ngoài (Google) xác nhận. KHÔNG chứa token của Google.</summary>
public sealed record ExternalIdentity(string Provider, string ProviderKey, string? Email, bool EmailVerified, string? Name);

public enum ExternalLoginOutcome
{
    SignedIn,
    SignedUpNew,
    EmailExistsNeedsPassword,
    EmailNotVerified,
    Unavailable,
}

public sealed record ExternalLoginResult(ExternalLoginOutcome Outcome, SessionTokens? Tokens);

/// <summary>
/// Đăng nhập bằng Google (UC-03, BR-AUTH-09). Tạo đúng loại phiên như đăng nhập thường. Không lưu token của Google.
/// </summary>
public sealed class ExternalLoginHandler(
    UserManager<AppUser> users,
    AppDbContext db,
    SessionService sessions,
    AuthEventLogger events,
    TimeProvider time,
    IOptions<AuthOptions> options)
{
    public async Task<ExternalLoginResult> HandleAsync(ExternalIdentity ext, ClientInfo client, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(ext.Email) || !ext.EmailVerified)
        {
            return new(ExternalLoginOutcome.EmailNotVerified, null);
        }

        var user = await users.FindByLoginAsync(ext.Provider, ext.ProviderKey);
        var isNew = false;

        if (user is null)
        {
            // BR-AUTH-09: trùng email với tài khoản đã có thì KHÔNG tự liên kết (tránh chiếm tài khoản).
            if (await users.FindByEmailAsync(ext.Email) is not null)
            {
                return new(ExternalLoginOutcome.EmailExistsNeedsPassword, null);
            }

            user = new AppUser
            {
                UserName = ext.Email,
                Email = ext.Email,
                EmailConfirmed = true, // Google đã xác minh email này
                DisplayName = DisplayNameFrom(ext),
                CreatedAt = time.GetUtcNow(),
            };

            var created = await users.CreateAsync(user);
            if (!created.Succeeded)
            {
                return new(ExternalLoginOutcome.Unavailable, null);
            }

            await users.AddToRoleAsync(user, Roles.Student);
            await users.AddLoginAsync(user, new UserLoginInfo(ext.Provider, ext.ProviderKey, ext.Provider));
            db.UserConsents.Add(new UserConsent
            {
                UserId = user.Id,
                TermsVersion = options.Value.TermsVersion,
                PrivacyVersion = options.Value.PrivacyVersion,
                MinorGuardianConfirmed = false,
                ConsentedAt = time.GetUtcNow(),
            });
            isNew = true;
        }
        else if (user.IsDeleted || (user.LockoutEnd is { } end && end > time.GetUtcNow()))
        {
            return new(ExternalLoginOutcome.Unavailable, null);
        }

        var roles = await users.GetRolesAsync(user);
        var session = await sessions.StartAsync(user, roles.Contains(Roles.Admin), rememberMe: false, ClientKind.Web, client, ct);
        events.Add(AuthEventKind.ExternalLoginSucceeded, user.Id, client);
        await db.SaveChangesAsync(ct);

        return new(isNew ? ExternalLoginOutcome.SignedUpNew : ExternalLoginOutcome.SignedIn, session);
    }

    private static string DisplayNameFrom(ExternalIdentity ext)
    {
        var name = (ext.Name ?? string.Empty).Trim();
        if (name.Length < 2)
        {
            name = ext.Email![..ext.Email!.IndexOf('@')];
        }

        name = name.Length > 50 ? name[..50] : name;
        return name.Length < 2 ? "Bạn mới" : name;
    }
}

public static class IdentityRedirects
{
    /// <summary>Chỉ nhận đường dẫn nội bộ làm returnUrl. Mọi thứ khác về trang mặc định (chống open redirect, ID-T14).</summary>
    public static string SafeReturnUrl(string? url, string fallback = "/")
        => string.IsNullOrEmpty(url) || !url.StartsWith('/') || url.StartsWith("//", StringComparison.Ordinal) || url.Contains((char)92)
            ? fallback
            : url;
}
