namespace Aduo.Core.Common.Paging;

/// <summary>
/// Tham số phân trang. Giá trị ngoài khoảng hợp lệ được kẹp lại thay vì báo lỗi
/// (page &lt; 1 → 1, pageSize trong [1, 100]) để client không bị lỗi vì tham số vô hại.
/// </summary>
public sealed record PageRequest
{
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;

    public PageRequest(int? page = null, int? pageSize = null)
    {
        Page = Math.Max(1, page ?? 1);
        PageSize = Math.Clamp(pageSize ?? DefaultPageSize, 1, MaxPageSize);
    }

    public int Page { get; }

    public int PageSize { get; }

    public int Skip => (Page - 1) * PageSize;
}

/// <summary>Kết quả một trang: { items, page, pageSize, totalCount } (docs/phase-3/03-api-design.md §1.3).</summary>
public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount);
