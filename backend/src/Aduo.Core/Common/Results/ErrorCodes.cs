namespace Aduo.Core.Common.Results;

/// <summary>
/// Mã lỗi dùng chung. Mỗi module tự khai báo thêm mã của mình (ví dụ SECTION_CLOSED trong Assessment).
/// Danh sách đầy đủ: docs/phase-3/03-api-design.md §1.4.
/// </summary>
public static class ErrorCodes
{
    public const string ValidationFailed = "VALIDATION_FAILED";
    public const string InvalidSort = "INVALID_SORT";
    public const string Unauthenticated = "UNAUTHENTICATED";
    public const string Forbidden = "FORBIDDEN";
    public const string NotFound = "NOT_FOUND";
    public const string ConcurrencyConflict = "CONCURRENCY_CONFLICT";
    public const string RateLimited = "RATE_LIMITED";
    public const string InternalError = "INTERNAL_ERROR";
}
