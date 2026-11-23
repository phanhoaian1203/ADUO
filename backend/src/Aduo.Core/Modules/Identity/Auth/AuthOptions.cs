using System.Security.Cryptography;
using Aduo.Core.Modules.Identity.Domain;
using Microsoft.IdentityModel.Tokens;

namespace Aduo.Core.Modules.Identity.Auth;

/// <summary>Cấu hình JWT. Bind từ mục "Jwt" trong appsettings / biến môi trường (Jwt__Keys__0__Secret…).</summary>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "aduo";

    public string Audience { get; set; } = "aduo-web";

    /// <summary>Thời hạn access token (phút). BR-AUTH-10: 10 phút.</summary>
    public int AccessTokenMinutes { get; set; } = 10;

    /// <summary>
    /// Danh sách khóa. Khóa ĐẦU TIÊN dùng để ký, mọi khóa trong danh sách đều dùng để xác minh.
    /// Nhờ vậy xoay khóa không làm gián đoạn người dùng (docs 01-identity §14.8).
    /// </summary>
    public List<JwtKeyOptions> Keys { get; set; } = [];

    /// <summary>Bắt buộc ≥ 256 bit. Fail-fast khi khởi động nếu cấu hình sai, để không chạy với khóa yếu.</summary>
    public IReadOnlyList<SymmetricSecurityKey> BuildKeys()
    {
        if (Keys.Count == 0)
        {
            throw new InvalidOperationException(
                "Thiếu cấu hình Jwt:Keys. Đặt biến môi trường Jwt__Keys__0__KeyId và Jwt__Keys__0__Secret (≥ 32 ký tự).");
        }

        var result = new List<SymmetricSecurityKey>(Keys.Count);
        foreach (var k in Keys)
        {
            if (string.IsNullOrWhiteSpace(k.KeyId))
            {
                throw new InvalidOperationException("Mỗi khóa JWT phải có KeyId.");
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(k.Secret ?? string.Empty);
            if (bytes.Length < 32)
            {
                throw new InvalidOperationException($"Khóa JWT '{k.KeyId}' quá ngắn: cần ít nhất 32 byte (256 bit).");
            }

            result.Add(new SymmetricSecurityKey(bytes) { KeyId = k.KeyId });
        }

        return result;
    }
}

public sealed class JwtKeyOptions
{
    public string KeyId { get; set; } = string.Empty;

    public string Secret { get; set; } = string.Empty;
}

/// <summary>Cấu hình cookie và thời hạn phiên. Mục "Auth" trong appsettings.</summary>
public sealed class AuthOptions
{
    public const string SectionName = "Auth";

    /// <summary>Gắn cờ Secure cho cookie. Chỉ tắt ở dev/test chạy trên http://localhost.</summary>
    public bool UseSecureCookies { get; set; } = true;

    /// <summary>Dùng tiền tố __Host-/__Secure-. Chỉ bật ở staging/production (cần HTTPS). Docs 01-identity §14.7.</summary>
    public bool UseCookiePrefixes { get; set; } = true;

    /// <summary>URL gốc của web (dùng dựng link trong email).</summary>
    public string AppBaseUrl { get; set; } = "http://localhost:3000";

    public string TermsVersion { get; set; } = "2026-11";

    public string PrivacyVersion { get; set; } = "2026-11";

    /// <summary>Ân hạn khi hai tab cùng làm mới (giây). Docs §14.5.</summary>
    public int RefreshGraceSeconds { get; set; } = 10;

    /// <summary>Thời gian giữ bộ đệm trạng thái phiên (giây). BR-AUTH-15: tối đa 60 giây.</summary>
    public int SessionCacheSeconds { get; set; } = 60;

    /// <summary>Endpoint Admin yêu cầu đăng nhập trong vòng N giờ (BR-AUTH-13).</summary>
    public int AdminReauthHours { get; set; } = 12;

    public int AuthEventRetentionDays { get; set; } = 180;

    public int DeletedAccountGraceDays { get; set; } = 7;

    /// <summary>Nếu không rỗng, request ghi có header Origin phải nằm trong danh sách (ví dụ https://aduo.vn).</summary>
    public List<string> AllowedOrigins { get; set; } = [];

    public string AccessCookieName => UseCookiePrefixes ? "__Host-aduo_at" : "aduo_at";

    public string RefreshCookieName => UseCookiePrefixes ? "__Secure-aduo_rt" : "aduo_rt";

    /// <summary>Cookie refresh chỉ được gửi tới các endpoint xác thực (không gửi kèm mọi request).</summary>
    public string RefreshCookiePath { get; set; } = "/api/v1/auth";

    public string AccessCookiePath => "/";
}

/// <summary>Chính sách thời hạn của một loại phiên (docs 01-identity §14.4).</summary>
public sealed record SessionPolicy(TimeSpan Idle, TimeSpan Absolute, bool PersistentCookie);

public static class SessionPolicies
{
    public static SessionPolicy For(bool isAdmin, bool rememberMe, ClientKind kind)
    {
        if (isAdmin)
        {
            // Admin: không có "Ghi nhớ", tối đa 12 giờ (BR-AUTH-13).
            return new SessionPolicy(TimeSpan.FromHours(2), TimeSpan.FromHours(12), PersistentCookie: false);
        }

        if (kind == ClientKind.Native)
        {
            return new SessionPolicy(TimeSpan.FromDays(30), TimeSpan.FromDays(90), PersistentCookie: true);
        }

        return rememberMe
            ? new SessionPolicy(TimeSpan.FromDays(30), TimeSpan.FromDays(90), PersistentCookie: true)
            : new SessionPolicy(TimeSpan.FromHours(24), TimeSpan.FromDays(7), PersistentCookie: false);
    }
}

public static class TokenHashing
{
    /// <summary>Sinh refresh token ngẫu nhiên 256 bit (CSPRNG), dạng base64url.</summary>
    public static string NewRefreshToken() => Base64Url(RandomNumberGenerator.GetBytes(32));

    /// <summary>SHA-256 của token. Chỉ lưu giá trị này, không bao giờ lưu token gốc.</summary>
    public static byte[] Hash(string token) => SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(token));

    /// <summary>Dấu vết ngắn (8 byte) của SecurityStamp để nhúng vào access token mà không lộ stamp.</summary>
    public static string StampFingerprint(string? securityStamp)
        => Base64Url(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(securityStamp ?? string.Empty))[..8]);

    private static string Base64Url(byte[] bytes)
        => Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}
