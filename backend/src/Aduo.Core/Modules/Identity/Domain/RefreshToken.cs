namespace Aduo.Core.Modules.Identity.Domain;

public enum ClientKind
{
    Web,
    Native,
}

/// <summary>Lý do thu hồi một phiên. Lưu dạng text để đọc được khi điều tra.</summary>
public enum RevokeReason
{
    Logout,
    LogoutAll,
    PasswordChanged,
    ReuseDetected,
    AdminLock,
    AccountDeleted,
    UserRevokedDevice,
}

/// <summary>
/// Một refresh token. Chuỗi các token xoay vòng của một phiên đăng nhập có chung <see cref="FamilyId"/>
/// (chính là <c>sid</c> trong access token). Server chỉ lưu BĂM SHA-256 của token (docs 01-identity §14.2).
/// </summary>
public sealed class RefreshToken
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public Guid UserId { get; set; }

    public Guid FamilyId { get; set; }

    public byte[] TokenHash { get; set; } = [];

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset LastUsedAt { get; set; }

    /// <summary>Lần nhập mật khẩu (hoặc đăng nhập Google) gần nhất của phiên. Giữ nguyên khi xoay vòng, dùng cho yêu cầu đăng nhập lại của Admin (BR-AUTH-13).</summary>
    public DateTimeOffset AuthTime { get; set; }

    /// <summary>Hạn nghỉ, tính lại sau mỗi lần xoay vòng.</summary>
    public DateTimeOffset IdleExpiresAt { get; set; }

    /// <summary>Hạn tuyệt đối của cả chuỗi. KHÔNG đổi giữa các lần xoay vòng (BR-AUTH-12).</summary>
    public DateTimeOffset AbsoluteExpiresAt { get; set; }

    /// <summary>Đã được đổi lấy token mới lúc nào. Null nghĩa là chưa dùng.</summary>
    public DateTimeOffset? UsedAt { get; set; }

    public Guid? ReplacedById { get; set; }

    public DateTimeOffset? RevokedAt { get; set; }

    public RevokeReason? RevokedReason { get; set; }

    public ClientKind ClientKind { get; set; }

    public bool RememberMe { get; set; }

    public bool IsAdminSession { get; set; }

    /// <summary>IP đã rút gọn (IPv4 /24, IPv6 /48). Không bao giờ lưu IP đầy đủ.</summary>
    public string? IpPrefix { get; set; }

    public string? UserAgent { get; set; }

    public string? DeviceLabel { get; set; }

    public bool IsRevoked => RevokedAt is not null;
}
