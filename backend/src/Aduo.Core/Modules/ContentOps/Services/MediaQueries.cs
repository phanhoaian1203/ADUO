using Aduo.Core.Infrastructure.Persistence;
using Aduo.Core.Infrastructure.Storage;
using Aduo.Core.Modules.ContentOps.Domain;
using Microsoft.EntityFrameworkCore;

namespace Aduo.Core.Modules.ContentOps.Services;

public sealed record MediaPage(IReadOnlyList<MediaResponse> Items, int Total, int Page, int PageSize);

/// <summary>Liệt kê và đọc file media cho màn hình quản trị.</summary>
public sealed class MediaQueries(AppDbContext db, IObjectStorage storage)
{
    public static MediaResponse ToResponse(MediaAsset m)
        => new(m.Id, m.Kind, m.ContentType, m.OriginalFileName, m.SizeBytes, m.ChecksumSha256,
            m.Meta.SourceId, m.Meta.LicenseId, m.Attribution, m.OriginalUrl, m.CreatedAt);

    public async Task<MediaPage> ListAsync(MediaKind? kind, string? q, int page, int pageSize, CancellationToken ct)
    {
        var query = db.MediaAssets.AsNoTracking().AsQueryable();
        if (kind is not null)
        {
            query = query.Where(m => m.Kind == kind);
        }

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim().ToLowerInvariant();
            query = query.Where(m => (m.OriginalFileName ?? "").ToLower().Contains(term) || (m.Attribution ?? "").ToLower().Contains(term));
        }

        page = Math.Max(page, 1);
        var total = await query.CountAsync(ct);
        var rows = await query.OrderByDescending(m => m.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return new MediaPage(rows.Select(ToResponse).ToList(), total, page, pageSize);
    }

    /// <summary>Mở file để Admin nghe thử/xem trước. Trả null nếu không có.</summary>
    public async Task<(Stream Stream, string ContentType, string? FileName)?> OpenAsync(Guid id, CancellationToken ct)
    {
        var m = await db.MediaAssets.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct);
        if (m is null)
        {
            return null;
        }

        var stream = await storage.OpenReadAsync(m.StorageKey, ct);
        return stream is null ? null : (stream, m.ContentType, m.OriginalFileName);
    }
}
