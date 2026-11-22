using Microsoft.AspNetCore.Hosting;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Aduo.Core.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Aduo.Api.IntegrationTests;

/// <summary>
/// Test case F-T01, F-T03, F-T08, F-T09 của docs/phase-4/modules/00-foundation.md §5.
/// Không cần database nên không cần Docker.
/// </summary>
[Collection(NoDbCollection.Name)]
public class FoundationHttpTests(NoDbApiFactory factory)
{
    [Fact]
    public async Task Health_ReturnsHealthy() // F-T01
    {
        var response = await factory.CreateClient().GetAsync("/api/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Healthy", body.GetProperty("status").GetString());
    }

    [Fact]
    public async Task UnhandledException_Returns500ProblemDetails_WithoutInternalDetails() // F-T03
    {
        var response = await factory.CreateClient().GetAsync("/api/_test/throw");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var raw = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("secret internal detail", raw);
        Assert.DoesNotContain("InvalidOperationException", raw);

        var problem = JsonDocument.Parse(raw).RootElement;
        Assert.Equal("INTERNAL_ERROR", problem.GetProperty("code").GetString());
        Assert.False(string.IsNullOrEmpty(problem.GetProperty("traceId").GetString()));
    }

    [Fact]
    public async Task UnknownRoute_Returns404ProblemDetails_WithCode()
    {
        var response = await factory.CreateClient().GetAsync("/api/does-not-exist");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("NOT_FOUND", problem.GetProperty("code").GetString());
    }

    [Fact]
    public async Task Responses_HaveSecurityHeaders_AndTraceId() // F-T09
    {
        var response = await factory.CreateClient().GetAsync("/api/health");

        Assert.Equal("nosniff", response.Headers.GetValues("X-Content-Type-Options").Single());
        Assert.Equal("DENY", response.Headers.GetValues("X-Frame-Options").Single());
        Assert.Contains("frame-ancestors 'none'", response.Headers.GetValues("Content-Security-Policy").Single());
        Assert.True(response.Headers.Contains("X-Trace-Id"));
        // Môi trường Testing không phải Development, nên có HSTS.
        Assert.True(response.Headers.Contains("Strict-Transport-Security"));
    }

    [Fact]
    public async Task RateLimit_Returns429_WithRetryAfter() // F-T08
    {
        using var limited = factory.WithWebHostBuilder(b => b.UseSetting("RateLimiting:DefaultPerMinute", "3"));
        var client = limited.CreateClient();

        HttpResponseMessage? last = null;
        for (var i = 0; i < 4; i++)
        {
            last = await client.GetAsync("/api/does-not-exist"); // endpoint bất kỳ có áp rate limit
        }

        Assert.Equal(HttpStatusCode.TooManyRequests, last!.StatusCode);
        Assert.True(last.Headers.Contains("Retry-After"));
        var problem = await last.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("RATE_LIMITED", problem.GetProperty("code").GetString());
    }

    [Fact]
    public async Task Health_IsNotRateLimited()
    {
        using var limited = factory.WithWebHostBuilder(b => b.UseSetting("RateLimiting:DefaultPerMinute", "1"));
        var client = limited.CreateClient();

        for (var i = 0; i < 5; i++)
        {
            Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/health")).StatusCode);
        }
    }

    [Fact]
    public async Task Ready_Returns503_WhenDatabaseUnreachable()
    {
        var client = factory.CreateClient();

        var ready = await client.GetAsync("/api/health/ready");
        var live = await client.GetAsync("/api/health");

        Assert.Equal(HttpStatusCode.ServiceUnavailable, ready.StatusCode); // F-T02 (không cần Docker: DB không tồn tại)
        Assert.Equal(HttpStatusCode.OK, live.StatusCode); // liveness vẫn OK: tiến trình còn sống
    }
}

/// <summary>Cần PostgreSQL thật (Testcontainers) nên cần Docker Desktop đang chạy.</summary>
[Collection(ApiCollection.Name)]
public class FoundationDatabaseTests(ApiFactory factory)
{
    [Fact]
    public async Task Ready_ReturnsHealthy_WhenDatabaseIsUp()
    {
        var response = await factory.CreateClient().GetAsync("/api/health/ready");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Migrations_Apply_AndCreateAllModuleSchemas()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await db.Database.MigrateAsync();

        var schemas = await db.Database
            .SqlQuery<string>($"SELECT schema_name AS \"Value\" FROM information_schema.schemata")
            .ToListAsync();
        foreach (var expected in Schemas.All)
        {
            Assert.Contains(expected, schemas);
        }

        var extensions = await db.Database
            .SqlQuery<string>($"SELECT extname AS \"Value\" FROM pg_extension")
            .ToListAsync();
        Assert.Contains("pg_trgm", extensions);
        Assert.Contains("unaccent", extensions);
    }
}
