using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.WebUtilities;

namespace Aduo.Core.Modules.Identity.Auth;

public enum TokenCheck
{
    Valid,
    Expired,
    Invalid,
}

/// <summary>
/// Token một lần dùng cho link trong email (xác minh email, đặt lại mật khẩu).
/// Tự viết trên Data Protection thay vì token provider của Identity, vì provider đó đọc đồng hồ hệ thống
/// nên không kiểm thử được hết hạn bằng <see cref="TimeProvider"/> (ID-T07).
/// "binding" gắn token vào trạng thái hiện tại của người dùng: đặt lại mật khẩu gắn với SecurityStamp
/// nên đổi mật khẩu xong là token cũ chết, dùng được đúng một lần.
/// </summary>
public sealed class EmailTokenService(IDataProtectionProvider provider, TimeProvider time)
{
    public const string ConfirmEmail = "confirm-email";
    public const string ResetPassword = "reset-password";

    private readonly IDataProtector _protector = provider.CreateProtector("Aduo.Identity.EmailToken.v1");

    public string Create(string purpose, Guid userId, string binding, TimeSpan lifetime)
    {
        var expires = time.GetUtcNow().Add(lifetime).ToUnixTimeSeconds();
        var payload = $"{purpose}|{userId:N}|{expires}|{binding}";
        return WebEncoders.Base64UrlEncode(System.Text.Encoding.UTF8.GetBytes(_protector.Protect(payload)));
    }

    public TokenCheck Validate(string purpose, Guid userId, string binding, string? token)
    {
        if (string.IsNullOrEmpty(token))
        {
            return TokenCheck.Invalid;
        }

        try
        {
            var protectedText = System.Text.Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
            var parts = _protector.Unprotect(protectedText).Split('|', 4);
            if (parts.Length != 4 || parts[0] != purpose || parts[1] != userId.ToString("N") || parts[3] != binding
                || !long.TryParse(parts[2], out var exp))
            {
                return TokenCheck.Invalid;
            }

            return time.GetUtcNow().ToUnixTimeSeconds() > exp ? TokenCheck.Expired : TokenCheck.Valid;
        }
        catch (Exception ex) when (ex is FormatException or System.Security.Cryptography.CryptographicException)
        {
            return TokenCheck.Invalid;
        }
    }
}
