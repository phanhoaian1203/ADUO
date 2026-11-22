using System.Diagnostics;
using Aduo.Core.Common.Results;

namespace Aduo.Api.Setup;

/// <summary>
/// Chuẩn hóa MỌI response lỗi theo RFC 9457, kể cả lỗi do framework sinh ra (404 route, 405, 500…):
/// - luôn có "traceId" để đối chiếu với log;
/// - luôn có "code" (nếu handler chưa đặt thì suy ra từ status);
/// - lỗi 500 không bao giờ lộ chi tiết exception ra ngoài.
/// </summary>
public static class ProblemDetailsSetup
{
    public static IServiceCollection AddAduoProblemDetails(this IServiceCollection services)
    {
        services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = ctx =>
            {
                var problem = ctx.ProblemDetails;
                problem.Extensions["traceId"] = Activity.Current?.TraceId.ToString() ?? ctx.HttpContext.TraceIdentifier;

                if (!problem.Extensions.ContainsKey("code"))
                {
                    problem.Extensions["code"] = problem.Status switch
                    {
                        StatusCodes.Status400BadRequest => ErrorCodes.ValidationFailed,
                        StatusCodes.Status401Unauthorized => ErrorCodes.Unauthenticated,
                        StatusCodes.Status403Forbidden => ErrorCodes.Forbidden,
                        StatusCodes.Status404NotFound => ErrorCodes.NotFound,
                        StatusCodes.Status429TooManyRequests => ErrorCodes.RateLimited,
                        _ => ErrorCodes.InternalError,
                    };
                }

                if (problem.Status >= StatusCodes.Status500InternalServerError)
                {
                    problem.Title = "Đã có lỗi xảy ra. Vui lòng thử lại sau.";
                    problem.Detail = null;
                    problem.Extensions.Remove("exception");
                }
            };
        });

        return services;
    }
}
