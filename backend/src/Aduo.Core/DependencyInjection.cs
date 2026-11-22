using Aduo.Core.Infrastructure.Jobs;
using Aduo.Core.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Aduo.Core;

/// <summary>
/// Điểm đăng ký duy nhất của Aduo.Core. Aduo.Api và Aduo.Cli đều gọi AddAduoCore(), nên dùng chung cấu hình.
/// Mỗi module sau này thêm một dòng services.AddXxxModule() ở đây.
/// </summary>
public static class DependencyInjection
{
    public const string ConnectionStringName = "Aduo";

    public static IServiceCollection AddAduoCore(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(ConnectionStringName);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"Thiếu ConnectionStrings:{ConnectionStringName}. Xem README (user-secrets hoặc biến môi trường ConnectionStrings__{ConnectionStringName}).");
        }

        services.AddSingleton(TimeProvider.System);
        services.AddScoped<AuditInterceptor>();

        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString, npgsql =>
                {
                    npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                    npgsql.MigrationsHistoryTable("__ef_migrations_history", "public");
                })
                .UseSnakeCaseNamingConvention()
                .AddInterceptors(sp.GetRequiredService<AuditInterceptor>());
        });

        services.AddScoped<IJobLock, PostgresAdvisoryJobLock>();

        return services;
    }

    /// <summary>Bật host chạy job nền. Chỉ API gọi hàm này. CLI không chạy job.</summary>
    public static IServiceCollection AddAduoBackgroundJobs(this IServiceCollection services)
    {
        services.AddHostedService<RecurringJobHost>();
        return services;
    }
}
