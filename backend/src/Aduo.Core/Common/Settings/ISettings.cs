namespace Aduo.Core.Common.Settings;

/// <summary>
/// Đọc các giá trị cấu hình của business rules (ngưỡng 60%, trần chi phí…) do Admin chỉnh được.
/// Có cache ngắn nên đọc thường xuyên không tốn truy vấn. Cài đặt ở module ContentOps.
/// </summary>
public interface ISettings
{
    Task<decimal> GetNumberAsync(string key, CancellationToken ct);

    async Task<int> GetIntAsync(string key, CancellationToken ct) => (int)Math.Round(await GetNumberAsync(key, ct));
}
