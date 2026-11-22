using System.Globalization;
using System.Threading.RateLimiting;
using Aduo.Core.Common.Results;
using Microsoft.AspNetCore.RateLimiting;

namespace Aduo.Api.Setup;

/// <summary>
/// Rate limiting (docs/phase-3/01 §8). Các policy theo tên, endpoint gắn bằng .RequireRateLimiting(name).
/// Giới hạn chung: 300 request/phút cho mỗi người dùng (nếu đã đăng nhập) hoặc mỗi IP.
/// Các policy riêng cho đăng nhập, đăng ký… được thêm ở module Identity.
/// </summary>
public static class RateLimitingSetup
{
    public const string Default = "api-default";

    public static IServiceCollection AddAduoRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        var permitPerMinute = configuration.GetValue("RateLimiting:DefaultPerMinute", 300);

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(ctx =>
                RateLimitPartition.GetFixedWindowLimiter(
                    PartitionKey(ctx),
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = permitPerMinute,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0,
                    }));

            options.OnRejected = async (context, ct) =>
            {
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    context.HttpContext.Response.Headers.RetryAfter =
                        ((int)retryAfter.TotalSeconds).ToString(CultureInfo.InvariantCulture);
                }

                await Results.Problem(
                        title: "Bạn thao tác quá nhanh. Vui lòng thử lại sau ít phút.",
                        statusCode: StatusCodes.Status429TooManyRequests,
                        extensions: new Dictionary<string, object?> { ["code"] = ErrorCodes.RateLimited })
                    .ExecuteAsync(context.HttpContext);
            };
        });

        return services;
    }

    /// <summary>Đã đăng nhập thì theo userId, chưa đăng nhập thì theo IP.</summary>
    internal static string PartitionKey(HttpContext ctx)
    {
        var userId = ctx.User.Identity?.IsAuthenticated == true ? ctx.User.FindFirst("sub")?.Value : null;
        return userId is not null
            ? "u:" + userId
            : "ip:" + (ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown");
    }
}
