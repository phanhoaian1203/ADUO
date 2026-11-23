using System.Net.Mail;
using Aduo.Core.Common.Results;
using Aduo.Core.Common.Security;
using Aduo.Core.Modules.Identity.Domain;

namespace Aduo.Core.Modules.Identity.Features;

public sealed record RegisterRequest(
    string? DisplayName,
    string? Email,
    string? Password,
    bool AcceptTerms,
    string? TermsVersion,
    string? PrivacyVersion,
    bool MinorGuardianConfirmed = false);

public sealed record LoginRequest(string? Email, string? Password, bool RememberMe = false);

public sealed record ConfirmEmailRequest(Guid UserId, string? Token);

public sealed record ForgotPasswordRequest(string? Email);

public sealed record ResetPasswordRequest(string? Email, string? Token, string? NewPassword);

public sealed record ChangePasswordRequest(string? CurrentPassword, string? NewPassword);

public sealed record DeleteAccountRequest(string? Password, string? ConfirmText);

public sealed record UpdateProfileRequest(
    string? DisplayName,
    LearningPurpose? LearningPurpose,
    TargetLevel? TargetLevel,
    Guid? AvatarMediaId);

/// <summary>Hồ sơ của chính người dùng, kèm quyền để UI biết hiện/ẩn chức năng (server vẫn kiểm tra lại).</summary>
public sealed record MeResponse(
    Guid Id,
    string DisplayName,
    string Email,
    bool EmailConfirmed,
    bool HasPassword,
    LearningPurpose? LearningPurpose,
    TargetLevel? TargetLevel,
    Guid? AvatarMediaId,
    IReadOnlyList<string> Permissions,
    bool IsAdmin);

public sealed record SessionResponse(Guid Id, string? Device, string? IpPrefix, DateTimeOffset CreatedAt, DateTimeOffset LastUsedAt, bool Current);

public sealed record AdminUserResponse(Guid Id, string DisplayName, string Email, bool EmailConfirmed, bool IsLocked, bool IsAdmin, DateTimeOffset CreatedAt);

public static class IdentityErrors
{
    public static readonly Error InvalidCredentials = new("INVALID_CREDENTIALS", "Email hoặc mật khẩu không đúng.", ErrorType.Unauthorized);
    public static readonly Error EmailTaken = Error.Conflict("EMAIL_TAKEN", "Email này đã được đăng ký. Hãy đăng nhập.");
    public static readonly Error EmailNotVerified = Error.Forbidden("EMAIL_NOT_VERIFIED", "Bạn cần xác minh email để dùng chức năng này.");
    public static readonly Error TokenExpired = Error.Validation("TOKEN_EXPIRED", "Liên kết đã hết hạn. Hãy yêu cầu gửi lại.");
    public static readonly Error TokenInvalid = Error.Validation("TOKEN_INVALID", "Liên kết không hợp lệ hoặc đã được dùng.");
    public static readonly Error LastAdmin = Error.Rule("LAST_ADMIN", "Không thể khóa quản trị viên cuối cùng.");
    public static readonly Error WrongPassword = Error.Validation("WRONG_PASSWORD", "Mật khẩu hiện tại không đúng.");

    public static Error Locked(TimeSpan remaining)
        => new("ACCOUNT_LOCKED", "Tài khoản tạm thời bị khóa do đăng nhập sai nhiều lần. Hãy thử lại sau.", ErrorType.RateLimited)
        {
            FieldErrors = new Dictionary<string, string[]> { ["retryAfter"] = [((int)Math.Ceiling(remaining.TotalSeconds)).ToString()] },
        };
}

/// <summary>Kiểm tra dữ liệu đầu vào. Trả lỗi theo trường (BR-AUTH-01..04).</summary>
public static class IdentityValidation
{
    private static readonly string[] BannedNameParts = ["admin", "administrator", "aduo"];

    public static Dictionary<string, string[]> Register(RegisterRequest r)
    {
        var errors = new Dictionary<string, string[]>();
        AddIf(errors, "displayName", DisplayName(r.DisplayName));
        AddIf(errors, "email", Email(r.Email));
        if (string.IsNullOrEmpty(r.Password))
        {
            errors["password"] = ["Vui lòng nhập mật khẩu."];
        }

        if (!r.AcceptTerms)
        {
            errors["acceptTerms"] = ["Bạn cần đồng ý Điều khoản và Chính sách quyền riêng tư."];
        }

        return errors;
    }

    public static string? DisplayName(string? name)
    {
        var n = name?.Trim() ?? string.Empty;
        if (n.Length is < 2 or > 50)
        {
            return "Tên hiển thị dài từ 2 đến 50 ký tự.";
        }

        // Không cho đóng giả quản trị (lọc thô tục cơ bản có thể mở rộng bằng cấu hình sau).
        if (BannedNameParts.Any(p => string.Equals(n, p, StringComparison.OrdinalIgnoreCase)))
        {
            return "Tên hiển thị này không được phép dùng.";
        }

        return null;
    }

    public static string? Email(string? email)
    {
        var e = email?.Trim() ?? string.Empty;
        if (e.Length is 0 or > 254 || !MailAddress.TryCreate(e, out var addr) || addr.Address != e || !e.Contains('.', StringComparison.Ordinal))
        {
            return "Email không đúng định dạng.";
        }

        return null;
    }

    private static void AddIf(Dictionary<string, string[]> errors, string field, string? message)
    {
        if (message is not null)
        {
            errors[field] = [message];
        }
    }

    public static string NormalizeEmail(string? email) => (email ?? string.Empty).Trim();

    public static MeResponse ToMe(AppUser user, IEnumerable<string> roles, bool hasPassword)
    {
        var roleList = roles.ToList();
        var permissions = RolePermissionMap.For(roleList)
            .Where(p => !RolePermissionMap.RequiresVerifiedEmail(p) || user.EmailConfirmed)
            .Order(StringComparer.Ordinal)
            .ToList();

        return new MeResponse(
            user.Id, user.DisplayName, user.Email ?? string.Empty, user.EmailConfirmed, hasPassword,
            user.LearningPurpose, user.TargetLevel, user.AvatarMediaId, permissions, roleList.Contains(Roles.Admin));
    }
}
