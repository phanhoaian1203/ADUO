using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Aduo.Core.Infrastructure.Jobs;

/// <summary>
/// Chạy mọi <see cref="IRecurringJob"/> đã đăng ký, mỗi job một vòng lặp <see cref="PeriodicTimer"/> riêng.
/// - Mỗi lượt chạy tạo một DI scope mới (DbContext là scoped).
/// - Mỗi lượt bọc trong <see cref="IJobLock"/>.
/// - Lỗi của một job chỉ được log, không làm dừng tiến trình hay các job khác.
/// </summary>
public sealed class RecurringJobHost(
    IServiceScopeFactory scopeFactory,
    IEnumerable<RecurringJobRegistration> registrations,
    TimeProvider time,
    ILogger<RecurringJobHost> logger) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var loops = registrations.Select(r => RunLoopAsync(r, stoppingToken)).ToArray();
        return Task.WhenAll(loops);
    }

    private async Task RunLoopAsync(RecurringJobRegistration registration, CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(registration.Interval, time);
        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await RunOnceAsync(registration, stoppingToken);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Ứng dụng đang tắt: thoát bình thường.
        }
    }

    internal async Task RunOnceAsync(RecurringJobRegistration registration, CancellationToken ct)
    {
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var job = (IRecurringJob)scope.ServiceProvider.GetRequiredService(registration.JobType);
            var jobLock = scope.ServiceProvider.GetRequiredService<IJobLock>();

            var ran = await jobLock.TryRunExclusiveAsync(job.Name, job.RunAsync, ct);
            if (!ran)
            {
                logger.LogDebug("Job {Job} skipped: lock held elsewhere", job.Name);
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Job {JobType} failed", registration.JobType.Name);
        }
    }
}

/// <summary>Thông tin đăng ký một job (kiểu + chu kỳ) để host biết cần chạy gì.</summary>
public sealed record RecurringJobRegistration(Type JobType, TimeSpan Interval);

public static class RecurringJobServiceCollectionExtensions
{
    /// <summary>Đăng ký một job định kỳ. Job được resolve trong scope mới ở mỗi lượt chạy.</summary>
    public static IServiceCollection AddRecurringJob<TJob>(this IServiceCollection services, TimeSpan interval)
        where TJob : class, IRecurringJob
    {
        services.AddScoped<TJob>();
        services.AddSingleton(new RecurringJobRegistration(typeof(TJob), interval));
        return services;
    }
}
