using Aduo.Core.Common.Results;
using Microsoft.AspNetCore.Http;

namespace Aduo.Core.Common.Http;

/// <summary>
/// Chuyển <see cref="Result"/> / <see cref="Result{T}"/> sang HTTP response.
/// Mọi lỗi trả về theo RFC 9457 ProblemDetails, có thêm "code" (và "errors" cho validation).
/// Nhờ một chỗ duy nhất này, mọi endpoint xử lý lỗi giống hệt nhau.
/// </summary>
public static class ResultHttpExtensions
{
    public static int ToStatusCode(this ErrorType type) => type switch
    {
        ErrorType.Validation => StatusCodes.Status400BadRequest,
        ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
        ErrorType.Forbidden => StatusCodes.Status403Forbidden,
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        ErrorType.Rule => StatusCodes.Status422UnprocessableEntity,
        ErrorType.RateLimited => StatusCodes.Status429TooManyRequests,
        _ => StatusCodes.Status500InternalServerError,
    };

    public static IResult ToProblem(this Error error)
    {
        var extensions = new Dictionary<string, object?> { ["code"] = error.Code };
        if (error.FieldErrors is { Count: > 0 })
        {
            extensions["errors"] = error.FieldErrors;
        }

        return TypedResults.Problem(
            title: error.Message,
            statusCode: error.Type.ToStatusCode(),
            // URI tương đối: domain chưa chốt (Q-P3-03), RFC 9457 cho phép URI-reference.
            type: $"/errors/{error.Code.ToLowerInvariant().Replace('_', '-')}",
            extensions: extensions);
    }

    /// <summary>Thành công → 204 No Content.</summary>
    public static IResult ToHttpResult(this Result result)
        => result.IsSuccess ? TypedResults.NoContent() : result.Error.ToProblem();

    /// <summary>Thành công → 200 OK kèm giá trị (hoặc kết quả do <paramref name="onSuccess"/> quyết định, ví dụ 201).</summary>
    public static IResult ToHttpResult<T>(this Result<T> result, Func<T, IResult>? onSuccess = null)
    {
        if (!result.IsSuccess)
        {
            return result.Error.ToProblem();
        }

        return onSuccess is null ? TypedResults.Ok(result.Value) : onSuccess(result.Value);
    }
}
