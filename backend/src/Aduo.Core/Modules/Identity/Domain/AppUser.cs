using Microsoft.AspNetCore.Identity;

namespace Aduo.Core.Modules.Identity.Domain;

/// <summary>Mục đích học (PD-09). Lưu dạng text trong DB.</summary>
public enum LearningPurpose
{
    Ausbildung,
    Nursing,
    Recognition,
    Study,
    Other,
}

/// <summary>Cấp độ mục tiêu của người học.</summary>
public enum TargetLevel
{
    A1,
    A2,
    B1,
}

/// <summary>
/// Người dùng. Mở rộng IdentityUser để dùng sẵn băm mật khẩu, khóa tạm, token email, đăng nhập Google, SecurityStamp.
/// Khóa chính là UUID v7 (tăng theo thời gian, index B-tree tốt, không lộ số lượng người dùng).
/// </summary>
public sealed class AppUser : IdentityUser<Guid>
{
    public AppUser()
    {
        Id = Guid.CreateVersion7();
    }

    /// <summary>Tên hiển thị, 2–50 ký tự (BR-AUTH-03).</summary>
    public string DisplayName { get; set; } = string.Empty;

    public Guid? AvatarMediaId { get; set; }

    public LearningPurpose? LearningPurpose { get; set; }

    public TargetLevel? TargetLevel { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>Đặt khi người dùng yêu cầu xóa tài khoản. Tài khoản bị vô hiệu ngay (BR-PRIV-02).</summary>
    public DateTimeOffset? DeletedAt { get; set; }

    /// <summary>Đặt khi dữ liệu cá nhân đã được ẩn danh (sau thời gian ân hạn).</summary>
    public DateTimeOffset? AnonymizedAt { get; set; }

    public bool IsDeleted => DeletedAt is not null;
}

public sealed class AppRole : IdentityRole<Guid>
{
    public AppRole()
    {
        Id = Guid.CreateVersion7();
    }

    public AppRole(string name) : this()
    {
        Name = name;
        NormalizedName = name.ToUpperInvariant();
    }
}

/// <summary>
/// Bằng chứng đồng ý điều khoản (BR-AUTH-04). Mỗi lần đồng ý một phiên bản mới thêm một dòng, không ghi đè.
/// </summary>
public sealed class UserConsent
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public Guid UserId { get; set; }

    public string TermsVersion { get; set; } = string.Empty;

    public string PrivacyVersion { get; set; } = string.Empty;

    /// <summary>Người dùng xác nhận đã có cha mẹ/người giám hộ đồng ý (nếu dưới 16 tuổi).</summary>
    public bool MinorGuardianConfirmed { get; set; }

    public DateTimeOffset ConsentedAt { get; set; }
}
