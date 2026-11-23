namespace Aduo.Core.Modules.Identity.Domain;

public enum AuthEventKind
{
    LoginSucceeded,
    LoginFailed,
    LockedOut,
    Logout,
    LogoutAll,
    RefreshRotated,
    RefreshReuseDetected,
    PasswordChanged,
    PasswordResetRequested,
    PasswordResetCompleted,
    ConfirmationEmailSent,
    SessionRevoked,
    ExternalLoginSucceeded,
    AccountDeleted,
}

/// <summary>
/// Nhật ký sự kiện bảo mật (BR-AUTH-17). KHÔNG chứa mật khẩu, token hay email.
/// IP được rút gọn. Giữ 180 ngày rồi job dọn.
/// </summary>
public sealed class AuthEvent
{
    public long Id { get; set; }

    public Guid? UserId { get; set; }

    public AuthEventKind Kind { get; set; }

    public DateTimeOffset OccurredAt { get; set; }

    public string? IpPrefix { get; set; }

    public string? UserAgent { get; set; }

    /// <summary>Thông tin phụ không bí mật (ví dụ lý do). Lưu dạng jsonb.</summary>
    public string? Details { get; set; }
}
