using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Aduo.Api.IntegrationTests;

/// <summary>
/// API thật trong bộ nhớ, KHÔNG có database. Dùng cho các test không đụng DB
/// (định dạng lỗi, header bảo mật, rate limit, liveness) nên chạy nhanh và không cần Docker.
/// Connection string trỏ tới cổng không có ai nghe: nếu lỡ có code chạm DB thì test sẽ lỗi rõ ràng.
/// </summary>
public sealed class NoDbApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:Aduo", "Host=127.0.0.1;Port=1;Database=none;Username=none;Password=none;Timeout=1");
    }
}

[CollectionDefinition(Name)]
public sealed class NoDbCollection : ICollectionFixture<NoDbApiFactory>
{
    public const string Name = "api-no-db";
}
