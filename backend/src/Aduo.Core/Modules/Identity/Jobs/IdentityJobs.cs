using Aduo.Core.Common.Privacy;
using Aduo.Core.Infrastructure.Jobs;
using Aduo.Core.Infrastructure.Persistence;
using Aduo.Core.Modules.Identity.Auth;
using Aduo.Core.Modules.Identity.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Aduo.Core.Modules.Identity.Jobs;

/// <summary>
/// Ẩn danh tài khoản đã xóa quá hạn ân hạn (mặc định 7 ngày). Gọi mọi <see cref="IUserDataEraser"/> của các module khác
/// rồi mới ẩn danh chính bản ghi người dùng (BR-PRIV-02).
/// </summary>
public sealed class PurgeDeletedAccountsJob(
    AppDbContext db,
    IEnumerable<IUserDataEraser> erasers,
    TimeProvider time,
    IOptions<AuthOptions> options,
    ILogger<PurgeDeletedAccountsJob> logger) : IRecurringJob
{
    public string Name => "identity.purge-deleted-accounts";

    public TimeSpan Interval => TimeSpan.FromHours(6);

    public async Task RunAsync(CancellationToken ct)
    {
        var cutoff = time.GetUtcNow().AddDays(-options.Value.DeletedAccountGraceDays);
        var ids = await db.Users.AsNoTracking()
            .Where(u => u.DeletedAt != null && u.DeletedAt <= cutoff && u.AnonymizedAt == null)
            .Select(u => u.Id)
            .Take(100)
            .ToListAsync(ct);

        foreach (var id in ids)
        {
            // Identity chạy cuối cùng để nếu module khác lỗi thì lần sau thử lại (eraser phải idempotent).
            foreach (var eraser in erasers.OrderBy(e => e is IdentityDataEraser ? 1 : 0))
            {
                await eraser.EraseAsync(id, ct);
            }

            logger.LogInformation("Đã ẩn danh một tài khoản đã xóa");
        }
    }
}

/// <summary>Xóa phiên hết hạn và nhật ký bảo mật quá 180 ngày (BR-AUTH-17).</summary>
public sealed class CleanupExpiredSessionsJob(AppDbContext db, TimeProvider time, IOptions<AuthOptions> options) : IRecurringJob
{
    public string Name => "identity.cleanup-expired-sessions";

    public TimeSpan Interval => TimeSpan.FromHours(24);

    public async Task RunAsync(CancellationToken ct)
    {
        var now = time.GetUtcNow();
        // Giữ thêm 1 ngày sau khi hết hạn để còn phát hiện việc dùng lại token vừa hết hạn.
        await db.RefreshTokens.Where(t => t.AbsoluteExpiresAt < now.AddDays(-1)).ExecuteDeleteAsync(ct);
        var eventCutoff = now.AddDays(-options.Value.AuthEventRetentionDays);
        await db.AuthEvents.Where(e => e.OccurredAt < eventCutoff).ExecuteDeleteAsync(ct);
    }
}

/// <summary>Ẩn danh bản ghi người dùng: bỏ email/tên/mật khẩu/liên kết đăng nhập ngoài, giữ khóa chính để các bảng khác không mồ côi.</summary>
public sealed class IdentityDataEraser(AppDbContext db, TimeProvider time) : IUserDataEraser
{
    public string Module => "Identity";

    public async Task EraseAsync(Guid userId, CancellationToken ct)
    {
        var user = await db.Users.SingleOrDefaultAsync(u => u.Id == userId, ct);
        if (user is null || user.AnonymizedAt is not null)
        {
            return;
        }

        var placeholder = $"deleted-{userId:N}@deleted.invalid";
        user.UserName = placeholder;
        user.NormalizedUserName = placeholder.ToUpperInvariant();
        user.Email = placeholder;
        user.NormalizedEmail = placeholder.ToUpperInvariant();
        user.EmailConfirmed = false;
        user.DisplayName = "Người dùng đã xóa";
        user.PasswordHash = null;
        user.PhoneNumber = null;
        user.AvatarMediaId = null;
        user.LearningPurpose = null;
        user.TargetLevel = null;
        user.SecurityStamp = Guid.NewGuid().ToString();
        user.AnonymizedAt = time.GetUtcNow();

        await db.Set<IdentityUserLogin<Guid>>().Where(l => l.UserId == userId).ExecuteDeleteAsync(ct);
        await db.Set<IdentityUserToken<Guid>>().Where(t => t.UserId == userId).ExecuteDeleteAsync(ct);
        await db.Set<IdentityUserClaim<Guid>>().Where(c => c.UserId == userId).ExecuteDeleteAsync(ct);
        await db.RefreshTokens.Where(t => t.UserId == userId).ExecuteDeleteAsync(ct);
        await db.UserConsents.Where(c => c.UserId == userId).ExecuteDeleteAsync(ct);
        await db.AuthEvents.Where(e => e.UserId == userId).ExecuteUpdateAsync(
            s => s.SetProperty(e => e.UserAgent, (string?)null).SetProperty(e => e.IpPrefix, (string?)null), ct);

        await db.SaveChangesAsync(ct);
    }
}
