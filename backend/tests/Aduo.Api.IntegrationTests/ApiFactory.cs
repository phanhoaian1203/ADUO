using System.Collections.Concurrent;
using Aduo.Core.Infrastructure.Email;
using Aduo.Core.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Time.Testing;
using Testcontainers.PostgreSql;

namespace Aduo.Api.IntegrationTests;

/// <summary>
/// Dựng API thật trong bộ nhớ, nối tới một PostgreSQL thật chạy trong Docker (Testcontainers).
/// Test với DB thật để bắt được lỗi mà DB giả (in-memory) không có: ràng buộc, extension, SQL thật.
/// Yêu cầu: Docker Desktop đang chạy.
/// Đồng hồ là <see cref="FakeTimeProvider"/> để kiểm thử hết hạn token/phiên; email được ghi lại thay vì gửi.
/// </summary>
public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:17-alpine")
        .Build();

    public FakeTimeProvider Time { get; } = new(DateTimeOffset.UtcNow);

    public CapturingEmailSender Emails { get; } = new();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        // Áp toàn bộ migration lên DB trống, giống lúc deploy.
        using var scope = Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.MigrateAsync();
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:Aduo", _postgres.GetConnectionString());

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<TimeProvider>();
            services.AddSingleton<TimeProvider>(Time);
            services.RemoveAll<IEmailSender>();
            services.AddSingleton<IEmailSender>(Emails);
        });
    }
}

/// <summary>Ghi lại email "đã gửi" để test đọc link xác minh / đặt lại mật khẩu.</summary>
public sealed class CapturingEmailSender : IEmailSender
{
    public ConcurrentQueue<EmailMessage> Sent { get; } = new();

    public Task SendAsync(EmailMessage message, CancellationToken ct)
    {
        Sent.Enqueue(message);
        return Task.CompletedTask;
    }

    public EmailMessage? LastTo(string email) => Sent.Reverse().FirstOrDefault(m => string.Equals(m.To, email, StringComparison.OrdinalIgnoreCase));
}

[CollectionDefinition(Name)]
public sealed class ApiCollection : ICollectionFixture<ApiFactory>
{
    public const string Name = "api";
}
