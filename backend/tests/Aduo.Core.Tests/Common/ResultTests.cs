using Aduo.Core.Common.Http;
using Aduo.Core.Common.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Aduo.Core.Tests.Common;

public class ResultTests
{
    [Fact]
    public void ImplicitConversion_FromValue_IsSuccess()
    {
        Result<int> result = 42;
        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void ImplicitConversion_FromError_IsFailure()
    {
        Result<int> result = Error.NotFound();
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.NotFound, result.Error!.Code);
    }

    [Fact]
    public void Map_KeepsError()
    {
        Result<int> failed = Error.Rule("SOME_RULE", "x");
        var mapped = failed.Map(v => v.ToString());
        Assert.Equal("SOME_RULE", mapped.Error!.Code);
    }

    [Theory]
    [InlineData(ErrorType.Validation, 400)]
    [InlineData(ErrorType.Unauthorized, 401)]
    [InlineData(ErrorType.Forbidden, 403)]
    [InlineData(ErrorType.NotFound, 404)]
    [InlineData(ErrorType.Conflict, 409)]
    [InlineData(ErrorType.Rule, 422)]
    [InlineData(ErrorType.RateLimited, 429)]
    public void ErrorType_MapsToHttpStatus(ErrorType type, int expected)
        => Assert.Equal(expected, type.ToStatusCode());

    [Fact]
    public void FailedResult_ToHttpResult_IsProblemDetailsWithCode() // F-T04
    {
        Result<string> result = Error.NotFound("Không thấy bài học.");

        var http = Assert.IsType<ProblemHttpResult>(result.ToHttpResult());

        Assert.Equal(StatusCodes.Status404NotFound, http.StatusCode);
        Assert.Equal("Không thấy bài học.", http.ProblemDetails.Title);
        Assert.Equal(ErrorCodes.NotFound, http.ProblemDetails.Extensions["code"]);
    }

    [Fact]
    public void ValidationError_IncludesFieldErrors()
    {
        var fields = new Dictionary<string, string[]> { ["email"] = ["Email không hợp lệ"] };
        Result result = Error.Validation(ErrorCodes.ValidationFailed, "Dữ liệu không hợp lệ", fields);

        var http = Assert.IsType<ProblemHttpResult>(result.ToHttpResult());

        Assert.Equal(400, http.StatusCode);
        Assert.Same(fields, http.ProblemDetails.Extensions["errors"]);
    }

    [Fact]
    public void SuccessResult_ToHttpResult_IsOk()
    {
        Result<string> result = "hello";
        var http = Assert.IsType<Ok<string>>(result.ToHttpResult());
        Assert.Equal("hello", http.Value);
    }

    [Fact]
    public void SuccessResultWithoutValue_IsNoContent()
        => Assert.IsType<NoContent>(Result.Ok().ToHttpResult());
}
