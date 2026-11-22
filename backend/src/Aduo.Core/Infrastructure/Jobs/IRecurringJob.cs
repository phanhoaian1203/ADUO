namespace Aduo.Core.Infrastructure.Jobs;

/// <summary>
/// Tác vụ nền chạy định kỳ (ví dụ AutoSubmitExpiredAttempts mỗi phút).
/// Đăng ký bằng services.AddRecurringJob&lt;TJob&gt;(). <see cref="RecurringJobHost"/> sẽ chạy nó.
/// </summary>
public interface IRecurringJob
{
    /// <summary>Tên duy nhất. Được dùng làm khóa advisory lock để mỗi job chỉ chạy một nơi.</summary>
    string Name { get; }

    TimeSpan Interval { get; }

    Task RunAsync(CancellationToken cancellationToken);
}
