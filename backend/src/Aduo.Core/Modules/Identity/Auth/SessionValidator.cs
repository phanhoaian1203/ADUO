using System.Security.Claims;
using Aduo.Core.Common.Security;
using Aduo.Core.Infrastructure.Persistence;
using Aduo.Core.Modules.Identity.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Aduo.Core.Modules.Identity.Auth;

/// <summary>Trạng thái người dùng + phiên được đệm tối đa 60 giây (BR-AUTH-15).</summary>
public sealed record SessionStatus(
    bool Active,
    bool IsLocked,
    string StampFingerprint,
    bool EmailVerified,
    IReadOnlyList<string> Roles);

public static class SessionStatusCache
{
    public static string Key(Guid sessionId) => $"sess:{sessionId:N}";

    public static void Evict(IMemoryCache cache, Guid sessionId) => cache.Remove(Key(sessionId));
}

/// <summary>
/// Chạy SAU khi JWT đã được xác minh chữ ký/hạn: kiểm tra phiên còn hiệu lực, stamp khớp, người dùng chưa bị khóa/xóa,
/// rồi bổ sung claim vai trò/permission từ server (token không mang quyền).
/// </summary>
public sealed class SessionValidator(AppDbContext db, IMemoryCache cache, TimeProvider time, IOptions<AuthOptions> options)
{
    private readonly AuthOptions _opt = options.Value;

    /// <param name="bypassCache">Hành động nhạy cảm và Admin: bỏ qua bộ đệm để thu hồi có hiệu lực tức thì (BR-AUTH-15).</param>
    public async Task<SessionStatus?> GetStatusAsync(Guid sessionId, Guid userId, bool bypassCache, CancellationToken ct)
    {
        var key = SessionStatusCache.Key(sessionId);
        if (!bypassCache && cache.TryGetValue(key, out SessionStatus? cached) && cached is not null)
        {
            return cached;
        }

        var now = time.GetUtcNow();

        // Phiên còn sống nếu tồn tại một token của chuỗi chưa bị thu hồi và chưa hết hạn tuyệt đối.
        var alive = await db.RefreshTokens.AsNoTracking()
            .AnyAsync(t => t.FamilyId == sessionId && t.UserId == userId && t.RevokedAt == null && t.AbsoluteExpiresAt > now, ct);

        SessionStatus status;
        if (!alive)
        {
            status = new SessionStatus(false, false, string.Empty, false, []);
        }
        else
        {
            var user = await db.Users.AsNoTracking().SingleOrDefaultAsync(u => u.Id == userId, ct);
            if (user is null || user.IsDeleted)
            {
                status = new SessionStatus(false, false, string.Empty, false, []);
            }
            else
            {
                var roles = await (from ur in db.UserRoles.AsNoTracking()
                                   join r in db.Roles.AsNoTracking() on ur.RoleId equals r.Id
                                   where ur.UserId == userId
                                   select r.Name!).ToListAsync(ct);
                var locked = user.LockoutEnd is { } end && end > now;
                status = new SessionStatus(true, locked, TokenHashing.StampFingerprint(user.SecurityStamp), user.EmailConfirmed, roles);
            }
        }

        if (!bypassCache)
        {
            cache.Set(key, status, TimeSpan.FromSeconds(_opt.SessionCacheSeconds));
        }

        return status;
    }

    /// <summary>Tạo các claim bổ sung sau khi token hợp lệ.</summary>
    public static IEnumerable<Claim> BuildClaims(SessionStatus status)
    {
        foreach (var role in status.Roles)
        {
            yield return new Claim(ClaimTypes.Role, role);
        }

        foreach (var permission in RolePermissionMap.For(status.Roles))
        {
            if (RolePermissionMap.RequiresVerifiedEmail(permission) && !status.EmailVerified)
            {
                continue;
            }

            yield return new Claim(AduoClaims.Permission, permission);
        }

        yield return new Claim(AduoClaims.EmailVerified, status.EmailVerified ? "true" : "false");
    }
}
