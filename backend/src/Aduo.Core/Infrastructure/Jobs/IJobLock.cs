using System.Security.Cryptography;
using System.Text;
using Aduo.Core.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Aduo.Core.Infrastructure.Jobs;

/// <summary>Khóa phân tán đơn giản, để nếu sau này chạy 2 instance API thì mỗi job vẫn chỉ chạy ở một nơi.</summary>
public interface IJobLock
{
    /// <summary>Chạy <paramref name="work"/> nếu lấy được khóa. Trả false nếu khóa đang bị giữ ở nơi khác.</summary>
    Task<bool> TryRunExclusiveAsync(string name, Func<CancellationToken, Task> work, CancellationToken ct);
}

/// <summary>
/// Dùng pg_try_advisory_lock của PostgreSQL. Khóa gắn với connection, nên phải giữ cùng một
/// connection mở trong suốt thời gian job chạy, rồi unlock trên chính connection đó.
/// </summary>
public sealed class PostgresAdvisoryJobLock(AppDbContext db) : IJobLock
{
    public async Task<bool> TryRunExclusiveAsync(string name, Func<CancellationToken, Task> work, CancellationToken ct)
    {
        var key = ToLockKey(name);
        await db.Database.OpenConnectionAsync(ct);
        try
        {
            var acquired = await db.Database
                .SqlQuery<bool>($"SELECT pg_try_advisory_lock({key}) AS \"Value\"")
                .SingleAsync(ct);
            if (!acquired)
            {
                return false;
            }

            try
            {
                await work(ct);
                return true;
            }
            finally
            {
                await db.Database
                    .SqlQuery<bool>($"SELECT pg_advisory_unlock({key}) AS \"Value\"")
                    .SingleAsync(CancellationToken.None);
            }
        }
        finally
        {
            await db.Database.CloseConnectionAsync();
        }
    }

    /// <summary>Băm tên job thành số 64-bit ổn định (không dùng string.GetHashCode vì giá trị đổi theo mỗi lần chạy).</summary>
    internal static long ToLockKey(string name)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes("aduo-job:" + name));
        return BitConverter.ToInt64(hash, 0);
    }
}
