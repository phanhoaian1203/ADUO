using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.PostgreSql;

namespace Aduo.Api.IntegrationTests;

/// <summary>
/// Dựng API thật trong bộ nhớ, nối tới một PostgreSQL thật chạy trong Docker (Testcontainers).
/// Test với DB thật để bắt được lỗi mà DB giả (in-memory) không có: ràng buộc, extension, SQL thật.
/// Yêu cầu: Docker Desktop đang chạy.
/// </summary>
public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:17-alpine")
        .Build();

    public async Task InitializeAsync() => await _postgres.StartAsync();

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:Aduo", _postgres.GetConnectionString());
    }

}

[CollectionDefinition(Name)]
public sealed class ApiCollection : ICollectionFixture<ApiFactory>
{
    public const string Name = "api";
}
