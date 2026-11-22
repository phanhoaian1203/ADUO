namespace Aduo.Core.Common.Results;

/// <summary>
/// Loại lỗi nghiệp vụ. Mỗi loại map sang đúng một mã HTTP (xem ResultHttpExtensions).
/// </summary>
public enum ErrorType
{
    Validation, // 400
    Unauthorized, // 401
    Forbidden, // 403
    NotFound, // 404
    Conflict, // 409
    Rule, // 422 – vi phạm business rule
    RateLimited, // 429
}

/// <summary>
/// Lỗi nghiệp vụ dự đoán được. <see cref="Code"/> là mã ổn định để frontend xử lý (UPPER_SNAKE),
/// <see cref="Message"/> là câu tiếng Việt cho người dùng.
/// </summary>
public sealed record Error(string Code, string Message, ErrorType Type)
{
    /// <summary>Lỗi validation theo từng trường (chỉ dùng với <see cref="ErrorType.Validation"/>).</summary>
    public IReadOnlyDictionary<string, string[]>? FieldErrors { get; init; }

    public static Error Validation(string code, string message, IReadOnlyDictionary<string, string[]>? fields = null)
        => new(code, message, ErrorType.Validation) { FieldErrors = fields };

    public static Error NotFound(string message = "Không tìm thấy dữ liệu.")
        => new(ErrorCodes.NotFound, message, ErrorType.NotFound);

    public static Error Conflict(string code, string message) => new(code, message, ErrorType.Conflict);

    public static Error Rule(string code, string message) => new(code, message, ErrorType.Rule);

    public static Error Forbidden(string code, string message) => new(code, message, ErrorType.Forbidden);
}
