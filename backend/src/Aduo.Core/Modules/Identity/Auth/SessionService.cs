using Aduo.Core.Common.Results;
using Aduo.Core.Infrastructure.Persistence;
using Aduo.Core.Modules.Identity.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Aduo.Core.Modules.Identity.Auth;

/// <summary>Bộ token trả cho client sau khi đăng nhập hoặc làm mới.</summary>
public sealed record SessionTokens(
    string AccessToken,
    DateTimeOffset AccessExpiresAt,
    string? RefreshToken,
    DateTimeOffset RefreshExpiresAt,
    bool PersistentCookie,
    Guid UserId,
    Guid SessionId);

/// <summary>Một thiết bị/phiên đang đăng nhập, dùng cho màn "Thiết bị đăng nhập".</summary>
public sealed record SessionInfo(Guid SessionId, string? DeviceLabel, string? IpPrefix, DateTimeOffset CreatedAt, DateTimeOffset LastUsedAt, bool IsCurrent);

public static class AuthErrors
{
    public static readonly Error RefreshMissing = new("UNAUTHENTICATED", "Bạn chưa đăng nhập.", ErrorType.Unauthorized);
    public static readonly Error RefreshInvalid = new("UNAUTHENTICATED", "Phiên đăng nhập không hợp lệ. Vui lòng đăng nhập lại.", ErrorType.Unauthorized);
    public static readonly Error SessionExpired = new("UNAUTHENTICATED", "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.", ErrorType.Unauthorized);
    public static readonly Error RefreshReused = new("REFRESH_REUSED", "Phiên đăng nhập bị thu hồi vì lý do an toàn. Vui lòng đăng nhập lại.", ErrorType.Unauthorized);
    public static readonly Error SessionRevoked = new("SESSION_REVOKED", "Phiên đăng nhập đã bị thu hồi.", ErrorType.Unauthorized);
    public static readonly Error TokenExpired = new("TOKEN_EXPIRED", "Phiên truy cập đã hết hạn.", ErrorType.Unauthorized);
    public static readonly Error TokenRevoked = new("TOKEN_REVOKED", "Phiên truy cập không còn hiệu lực.", ErrorType.Unauthorized);
    public static readonly Error ReauthRequired = new("REAUTH_REQUIRED", "Vui lòng đăng nhập lại để tiếp tục.", ErrorType.Unauthorized);
    public static readonly Error CsrfInvalid = new("CSRF_INVALID", "Yêu cầu không hợp lệ. Hãy tải lại trang và thử lại.", ErrorType.Validation);
}

/// <summary>
/// Vòng đời phiên: tạo, xoay vòng refresh token (có phát hiện dùng lại), thu hồi.
/// Thiết kế: docs/phase-4/modules/01-identity.md §14.
/// </summary>
public sealed class SessionService(
    AppDbContext db,
    AccessTokenService accessTokens,
    AuthEventLogger events,
    IMemoryCache cache,
    TimeProvider time,
    IOptions<AuthOptions> options)
{
    private readonly AuthOptions _opt = options.Value;

    public async Task<SessionTokens> StartAsync(
        AppUser user,
        bool isAdmin,
        bool rememberMe,
        ClientKind kind,
        ClientInfo client,
        CancellationToken ct)
    {
        var now = time.GetUtcNow();
        var policy = SessionPolicies.For(isAdmin, rememberMe && !isAdmin, kind);
        var familyId = Guid.CreateVersion7();
        var raw = TokenHashing.NewRefreshToken();

        db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            FamilyId = familyId,
            TokenHash = TokenHashing.Hash(raw),
            CreatedAt = now,
            LastUsedAt = now,
            AuthTime = now,
            IdleExpiresAt = now + policy.Idle,
            AbsoluteExpiresAt = now + policy.Absolute,
            ClientKind = kind,
            RememberMe = rememberMe && !isAdmin,
            IsAdminSession = isAdmin,
            IpPrefix = client.IpPrefix,
            UserAgent = client.UserAgent,
            DeviceLabel = DeviceLabels.From(client.UserAgent),
        });

        await db.SaveChangesAsync(ct);
        return Build(user, familyId, raw, now, now + policy.Idle, now + policy.Absolute, policy.PersistentCookie, isAdmin);
    }

    /// <summary>
    /// Đổi refresh token cũ lấy cặp mới. Dùng lại token đã đổi ngoài ân hạn → thu hồi cả chuỗi (BR-AUTH-11).
    /// </summary>
    public async Task<Result<SessionTokens>> RefreshAsync(string? rawToken, ClientInfo client, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(rawToken))
        {
            return AuthErrors.RefreshMissing;
        }

        var now = time.GetUtcNow();
        var hash = TokenHashing.Hash(rawToken);

        await using var tx = await db.Database.BeginTransactionAsync(ct);

        // Khóa dòng để hai request đồng thời không cùng xoay một token (docs §14.5).
        var token = await db.RefreshTokens
            .FromSqlInterpolated($"SELECT * FROM identity.refresh_tokens WHERE token_hash = {hash} FOR UPDATE")
            .SingleOrDefaultAsync(ct);

        if (token is null)
        {
            return AuthErrors.RefreshInvalid;
        }

        if (token.IsRevoked)
        {
            return token.RevokedReason == RevokeReason.ReuseDetected ? AuthErrors.RefreshReused : AuthErrors.SessionRevoked;
        }

        if (token.UsedAt is { } usedAt)
        {
            // Trong ân hạn và đã có token kế nhiệm còn hiệu lực: coi là tab song song (BR-AUTH-11).
            // Chỉ cấp access token mới, KHÔNG xoay tiếp. Cookie refresh trình duyệt đã nhận từ request thắng cuộc.
            if (now - usedAt <= TimeSpan.FromSeconds(_opt.RefreshGraceSeconds) && token.ReplacedById is { } successorId)
            {
                var successor = await db.RefreshTokens.AsNoTracking().SingleOrDefaultAsync(t => t.Id == successorId, ct);
                if (successor is { IsRevoked: false } && successor.IdleExpiresAt > now && successor.AbsoluteExpiresAt > now)
                {
                    var parallelUser = await db.Users.AsNoTracking().SingleAsync(u => u.Id == token.UserId, ct);
                    await tx.CommitAsync(ct);
                    return Build(parallelUser, token.FamilyId, null, token.AuthTime, successor.IdleExpiresAt,
                        successor.AbsoluteExpiresAt, !successor.IsAdminSession && successor.RememberMe, successor.IsAdminSession);
                }
            }

            await RevokeFamilyCoreAsync(token.FamilyId, RevokeReason.ReuseDetected, now, ct);
            events.Add(AuthEventKind.RefreshReuseDetected, token.UserId, client);
            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return AuthErrors.RefreshReused;
        }

        if (token.IdleExpiresAt <= now || token.AbsoluteExpiresAt <= now)
        {
            return AuthErrors.SessionExpired;
        }

        var user = await db.Users.SingleOrDefaultAsync(u => u.Id == token.UserId, ct);
        if (user is null || user.IsDeleted || (user.LockoutEnd is { } end && end > now))
        {
            await RevokeFamilyCoreAsync(token.FamilyId, user?.IsDeleted == true ? RevokeReason.AccountDeleted : RevokeReason.AdminLock, now, ct);
            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return AuthErrors.SessionRevoked;
        }

        var policy = SessionPolicies.For(token.IsAdminSession, token.RememberMe, token.ClientKind);
        var raw = TokenHashing.NewRefreshToken();
        var next = new RefreshToken
        {
            UserId = token.UserId,
            FamilyId = token.FamilyId,
            TokenHash = TokenHashing.Hash(raw),
            CreatedAt = now,
            LastUsedAt = now,
            AuthTime = token.AuthTime,
            IdleExpiresAt = Min(now + policy.Idle, token.AbsoluteExpiresAt),
            AbsoluteExpiresAt = token.AbsoluteExpiresAt, // không gia hạn (BR-AUTH-12)
            ClientKind = token.ClientKind,
            RememberMe = token.RememberMe,
            IsAdminSession = token.IsAdminSession,
            IpPrefix = client.IpPrefix ?? token.IpPrefix,
            UserAgent = client.UserAgent ?? token.UserAgent,
            DeviceLabel = token.DeviceLabel,
        };

        token.UsedAt = now;
        token.LastUsedAt = now;
        token.ReplacedById = next.Id;
        db.RefreshTokens.Add(next);
        events.Add(AuthEventKind.RefreshRotated, token.UserId, client);

        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return Build(user, next.FamilyId, raw, token.AuthTime, next.IdleExpiresAt, next.AbsoluteExpiresAt,
            policy.PersistentCookie, token.IsAdminSession);
    }

    /// <summary>Thu hồi một phiên của chính người dùng (đăng xuất hoặc gỡ thiết bị).</summary>
    public async Task<bool> RevokeSessionAsync(Guid userId, Guid sessionId, RevokeReason reason, ClientInfo client, CancellationToken ct)
    {
        var owned = await db.RefreshTokens.AnyAsync(t => t.FamilyId == sessionId && t.UserId == userId, ct);
        if (!owned)
        {
            return false;
        }

        await RevokeFamilyCoreAsync(sessionId, reason, time.GetUtcNow(), ct);
        events.Add(reason == RevokeReason.Logout ? AuthEventKind.Logout : AuthEventKind.SessionRevoked, userId, client);
        await db.SaveChangesAsync(ct);
        return true;
    }

    /// <summary>Thu hồi mọi phiên của người dùng. <paramref name="exceptSessionId"/> giữ lại phiên hiện tại (đổi mật khẩu).</summary>
    public async Task RevokeAllAsync(Guid userId, RevokeReason reason, ClientInfo client, Guid? exceptSessionId, CancellationToken ct)
    {
        var now = time.GetUtcNow();
        var families = await db.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedAt == null && t.FamilyId != exceptSessionId)
            .Select(t => t.FamilyId)
            .Distinct()
            .ToListAsync(ct);

        foreach (var f in families)
        {
            await RevokeFamilyCoreAsync(f, reason, now, ct);
        }

        events.Add(AuthEventKind.LogoutAll, userId, client);
        await db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<SessionInfo>> ListAsync(Guid userId, Guid? currentSessionId, CancellationToken ct)
    {
        var now = time.GetUtcNow();

        // Mỗi phiên có nhiều token xoay vòng, chỉ token mới nhất (chưa dùng, chưa thu hồi) đại diện cho phiên.
        var rows = await db.RefreshTokens.AsNoTracking()
            .Where(t => t.UserId == userId && t.RevokedAt == null && t.UsedAt == null
                        && t.IdleExpiresAt > now && t.AbsoluteExpiresAt > now)
            .OrderByDescending(t => t.LastUsedAt)
            .ToListAsync(ct);

        return rows
            .Select(t => new SessionInfo(t.FamilyId, t.DeviceLabel, t.IpPrefix, t.CreatedAt, t.LastUsedAt, t.FamilyId == currentSessionId))
            .ToList();
    }

    private async Task RevokeFamilyCoreAsync(Guid familyId, RevokeReason reason, DateTimeOffset now, CancellationToken ct)
    {
        var tokens = await db.RefreshTokens.Where(t => t.FamilyId == familyId && t.RevokedAt == null).ToListAsync(ct);
        foreach (var t in tokens)
        {
            t.RevokedAt = now;
            t.RevokedReason = reason;
        }

        // Quên ngay trạng thái đã đệm để thu hồi có hiệu lực tức thì trên instance này.
        SessionStatusCache.Evict(cache, familyId);
    }

    private SessionTokens Build(AppUser user, Guid familyId, string? rawRefresh, DateTimeOffset authTime,
        DateTimeOffset idleExpires, DateTimeOffset absoluteExpires, bool persistent, bool isAdmin)
    {
        var (access, accessExp) = accessTokens.Create(user.Id, familyId, TokenHashing.StampFingerprint(user.SecurityStamp), authTime);
        return new SessionTokens(access, accessExp, rawRefresh, Min(idleExpires, absoluteExpires), persistent, user.Id, familyId);
    }

    private static DateTimeOffset Min(DateTimeOffset a, DateTimeOffset b) => a <= b ? a : b;
}

/// <summary>Nhãn thiết bị dễ đọc từ User-Agent (không phải phân tích đầy đủ, chỉ để người dùng nhận ra thiết bị).</summary>
public static class DeviceLabels
{
    public static string From(string? ua)
    {
        if (string.IsNullOrWhiteSpace(ua))
        {
            return "Thiết bị không xác định";
        }

        var os = ua.Contains("Android", StringComparison.OrdinalIgnoreCase) ? "Android"
            : ua.Contains("iPhone", StringComparison.OrdinalIgnoreCase) || ua.Contains("iPad", StringComparison.OrdinalIgnoreCase) ? "iOS"
            : ua.Contains("Windows", StringComparison.OrdinalIgnoreCase) ? "Windows"
            : ua.Contains("Mac OS", StringComparison.OrdinalIgnoreCase) ? "macOS"
            : ua.Contains("Linux", StringComparison.OrdinalIgnoreCase) ? "Linux"
            : "Thiết bị khác";

        var browser = ua.Contains("Edg/", StringComparison.OrdinalIgnoreCase) ? "Edge"
            : ua.Contains("Firefox", StringComparison.OrdinalIgnoreCase) ? "Firefox"
            : ua.Contains("Chrome", StringComparison.OrdinalIgnoreCase) ? "Chrome"
            : ua.Contains("Safari", StringComparison.OrdinalIgnoreCase) ? "Safari"
            : "Trình duyệt";

        return $"{browser} trên {os}";
    }
}
