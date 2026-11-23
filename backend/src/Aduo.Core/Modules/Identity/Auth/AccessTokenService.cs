using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Aduo.Core.Modules.Identity.Auth;

/// <summary>Tên các claim trong access token. Chỉ chứa thứ cần thiết, KHÔNG có email/tên/vai trò (docs §14.2).</summary>
public static class AduoClaims
{
    public const string Subject = "sub";
    public const string TokenId = "jti";
    public const string SessionId = "sid";
    public const string StampFingerprint = "sst";
    public const string AuthTime = "auth_time";

    // Claim do server thêm SAU khi xác minh token (không nằm trong JWT):
    public const string Permission = "permission";
    public const string EmailVerified = "email_verified";
}

/// <summary>Phát hành access token (JWT HS256, 10 phút).</summary>
public sealed class AccessTokenService
{
    private readonly JwtOptions _options;
    private readonly SigningCredentials _signing;
    private readonly TimeProvider _time;
    private readonly JsonWebTokenHandler _handler = new();

    public AccessTokenService(IOptions<JwtOptions> options, TimeProvider time)
    {
        _options = options.Value;
        _time = time;

        // Khóa đầu tiên dùng để ký.
        var keys = _options.BuildKeys();
        _signing = new SigningCredentials(keys[0], SecurityAlgorithms.HmacSha256);
    }

    public TimeSpan Lifetime => TimeSpan.FromMinutes(_options.AccessTokenMinutes);

    public (string Token, DateTimeOffset ExpiresAt) Create(Guid userId, Guid sessionId, string stampFingerprint, DateTimeOffset authTime)
    {
        var now = _time.GetUtcNow();
        var expires = now.Add(Lifetime);

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            IssuedAt = now.UtcDateTime,
            NotBefore = now.UtcDateTime,
            Expires = expires.UtcDateTime,
            SigningCredentials = _signing,
            Claims = new Dictionary<string, object>
            {
                [AduoClaims.Subject] = userId.ToString(),
                [AduoClaims.TokenId] = Guid.CreateVersion7().ToString(),
                [AduoClaims.SessionId] = sessionId.ToString(),
                [AduoClaims.StampFingerprint] = stampFingerprint,
                [AduoClaims.AuthTime] = authTime.ToUnixTimeSeconds(),
            },
        };

        return (_handler.CreateToken(descriptor), expires);
    }
}
