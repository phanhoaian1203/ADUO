namespace Aduo.Core.Common.Privacy;

/// <summary>
/// Mỗi module chứa dữ liệu cá nhân tự cài đặt interface này (BR-PRIV-02).
/// Identity không được đụng trực tiếp vào bảng của module khác (AR-03), nên job xóa tài khoản
/// gọi mọi <see cref="IUserDataEraser"/> đã đăng ký để mỗi module tự xóa hoặc ẩn danh dữ liệu của mình.
/// </summary>
public interface IUserDataEraser
{
    /// <summary>Tên module (dùng để log).</summary>
    string Module { get; }

    /// <summary>Xóa hoặc ẩn danh toàn bộ dữ liệu cá nhân của người dùng. Phải idempotent.</summary>
    Task EraseAsync(Guid userId, CancellationToken cancellationToken);
}
