using Aduo.Core.Infrastructure.Persistence;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Aduo.Api.Setup;

/// <summary>Kết quả health check. Có trong OpenAPI nên frontend có kiểu TypeScript tương ứng.</summary>
public sealed record HealthResponse(string Status, IReadOnlyDictionary<string, string> Checks);

/// <summary>
/// /api/health       – liveness: tiến trình còn sống, không kiểm tra gì thêm.
/// /api/health/ready – readiness: kết nối được PostgreSQL. Docker healthcheck và uptime monitor dùng endpoint này.
/// Viết bằng minimal API (thay vì MapHealthChecks) để endpoint xuất hiện trong OpenAPI.
/// </summary>
public static class HealthSetup
{
    private const string ReadyTag = "ready";

    public static IServiceCollection AddAduoHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks().AddDbContextCheck<AppDbContext>("database", tags: [ReadyTag]);
        return services;
    }

    public static IEndpointRouteBuilder MapAduoHealthChecks(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/health").WithTags("Health").DisableRateLimiting();

        group.MapGet("/", () => TypedResults.Ok(new HealthResponse("Healthy", new Dictionary<string, string>())))
            .WithName("GetHealth");

        group.MapGet("/ready", async (HealthCheckService health, CancellationToken ct) =>
            {
                var report = await health.CheckHealthAsync(c => c.Tags.Contains(ReadyTag), ct);
                var body = new HealthResponse(
                    report.Status.ToString(),
                    report.Entries.ToDictionary(e => e.Key, e => e.Value.Status.ToString()));

                return report.Status == HealthStatus.Healthy
                    ? Results.Ok(body)
                    : Results.Json(body, statusCode: StatusCodes.Status503ServiceUnavailable);
            })
            .WithName("GetReadiness")
            .Produces<HealthResponse>()
            .Produces<HealthResponse>(StatusCodes.Status503ServiceUnavailable);

        return app;
    }
}
