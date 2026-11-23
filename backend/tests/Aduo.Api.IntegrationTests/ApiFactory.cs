using System.Collections.Concurrent;
using Aduo.Core.Common.Content;
using Aduo.Core.Infrastructure.Email;
using Aduo.Core.Modules.ContentOps.Import;
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

    public FakeContentResource Content { get; } = new();

    private readonly string _storageDir = Path.Combine(Path.GetTempPath(), "aduo-test-storage-" + Guid.NewGuid().ToString("N"));

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
        if (Directory.Exists(_storageDir))
        {
            Directory.Delete(_storageDir, recursive: true);
        }
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:Aduo", _postgres.GetConnectionString());
        builder.UseSetting("Storage:LocalPath", _storageDir);

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<TimeProvider>();
            services.AddSingleton<TimeProvider>(Time);
            services.RemoveAll<IEmailSender>();
            services.AddSingleton<IEmailSender>(Emails);
            services.AddSingleton<IContentResource>(Content);
            services.AddScoped<IImportHandler, FakeImportHandler>();
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
