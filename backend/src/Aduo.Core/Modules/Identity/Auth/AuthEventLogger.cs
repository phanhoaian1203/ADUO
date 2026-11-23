using System.Net;
using Aduo.Core.Infrastructure.Persistence;
using Aduo.Core.Modules.Identity.Domain;

namespace Aduo.Core.Modules.Identity.Auth;

/// <summary>Thông tin client của một request, đã rút gọn để không lưu dữ liệu cá nhân thừa (BR-AUTH-17).</summary>
public sealed record ClientInfo(string? IpPrefix, string? UserAgent)
{
    public static readonly ClientInfo Unknown = new(null, null);

    public static ClientInfo From(IPAddress? ip, string? userAgent)
        => new(TruncateIp(ip), Truncate(userAgent, 200));

    /// <summary>IPv4 giữ /24, IPv6 giữ /48. Không bao giờ lưu IP đầy đủ.</summary>
    public static string? TruncateIp(IPAddress? ip)
    {
        if (ip is null)
        {
            return null;
        }

        if (ip.IsIPv4MappedToIPv6)
        {
            ip = ip.MapToIPv4();
        }

        var bytes = ip.GetAddressBytes();
        if (bytes.Length == 4)
        {
            return $"{bytes[0]}.{bytes[1]}.{bytes[2]}.0/24";
        }

        var prefix = new byte[16];
        Array.Copy(bytes, prefix, 6);
        return new IPAddress(prefix) + "/48";
    }

    private static string? Truncate(string? s, int max) => s is null ? null : s.Length <= max ? s : s[..max];
}

/// <summary>Ghi sự kiện bảo mật vào <c>identity.auth_events</c>. Không nhận email/token/mật khẩu.</summary>
public sealed class AuthEventLogger(AppDbContext db, TimeProvider time)
{
    public void Add(AuthEventKind kind, Guid? userId, ClientInfo client, string? detailsJson = null)
        => db.AuthEvents.Add(new AuthEvent
        {
            Kind = kind,
            UserId = userId,
            OccurredAt = time.GetUtcNow(),
            IpPrefix = client.IpPrefix,
            UserAgent = client.UserAgent,
            Details = detailsJson,
        });
}
