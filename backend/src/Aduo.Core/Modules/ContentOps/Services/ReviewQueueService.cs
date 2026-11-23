using Aduo.Core.Common.Content;
using Aduo.Core.Common.Results;
using Aduo.Core.Infrastructure.Persistence;
using Aduo.Core.Modules.ContentOps.Domain;
using Microsoft.EntityFrameworkCore;

namespace Aduo.Core.Modules.ContentOps.Services;

public sealed record ReviewQueuePage(IReadOnlyList<ReviewQueueItem> Items, int Total, int Page, int PageSize);

public sealed record BatchPublishItem(string Resource, Guid Id, int Version);

public sealed record BatchPublishOutcome(string Resource, Guid Id, bool Published, string? Code, IReadOnlyList<string>? Reasons);

public sealed record AttributionItem(
    Guid SourceId, string Name, string? Provider, string? Url,
    string LicenseCode, string LicenseName, string? LicenseUrl, bool RequiresAttribution, bool ShareAlike,
    int PublishedCount);

/// <summary>Hàng đợi duyệt gom từ mọi loại nội dung (US-54) và trang ghi công (FR-CMS-10).</summary>
public sealed class ReviewQueueService(AppDbContext db, LifecycleService lifecycle)
{
    public async Task<ReviewQueuePage> ListAsync(
        ReviewQueueQuery query, bool? flagged, int page, int pageSize, CancellationToken ct)
    {
        var items = new List<ReviewQueueItem>();
        foreach (var res in lifecycle.All.Where(r => query.Kind is null || r.Kind == query.Kind))
        {
            items.AddRange(await res.ListPendingAsync(query, ct));
        }

        // Có cờ lên đầu, rồi cũ nhất trước (cái đã chờ lâu nhất được xử lý trước).
        var ordered = items
            .Where(i => flagged is null || i.Flagged == flagged)
            .OrderByDescending(i => i.Flagged)
            .ThenBy(i => i.UpdatedAt)
            .ToList();

        page = Math.Max(page, 1);
        return new ReviewQueuePage(ordered.Skip((page - 1) * pageSize).Take(pageSize).ToList(), ordered.Count, page, pageSize);
    }

    /// <summary>
    /// Publish hàng loạt: chỉ cho các mục KHÔNG có cờ. Tick checklist một lần cho cả lô nhưng hệ thống vẫn ghi
    /// <c>review_checklists</c> cho từng mục (do <see cref="LifecycleService.PublishAsync"/>).
    /// </summary>
    public async Task<IReadOnlyList<BatchPublishOutcome>> PublishBatchAsync(
        IReadOnlyList<BatchPublishItem> items, ChecklistRequest? checklist, Guid reviewerId, CancellationToken ct)
    {
        var outcomes = new List<BatchPublishOutcome>();
        foreach (var it in items)
        {
            var res = lifecycle.FindResource(it.Resource);
            var current = res is null ? null : await res.FindAsync(it.Id, ct);
            if (current is null)
            {
                outcomes.Add(new(it.Resource, it.Id, false, "NOT_FOUND", null));
                continue;
            }

            if (current.Flags.Count > 0 || current.Meta.Status == ContentStatus.NeedsUpdate)
            {
                outcomes.Add(new(it.Resource, it.Id, false, "FLAGGED", current.Flags));
                continue;
            }

            var result = await lifecycle.PublishAsync(it.Resource, it.Id, it.Version, checklist, reviewerId, ct);
            outcomes.Add(result.IsSuccess
                ? new(it.Resource, it.Id, true, null, null)
                : new(it.Resource, it.Id, false, result.Error.Code, result.Error.FieldErrors?.GetValueOrDefault("reasons")));
        }

        return outcomes;
    }

    /// <summary>Chỉ liệt kê nguồn đang có nội dung Published (CO-T19). Là nghĩa vụ ghi công của CC BY / CC BY-SA.</summary>
    public async Task<IReadOnlyList<AttributionItem>> AttributionsAsync(CancellationToken ct)
    {
        var counts = new Dictionary<Guid, int>();
        foreach (var res in lifecycle.All)
        {
            foreach (var (sourceId, n) in await res.CountPublishedBySourceAsync(ct))
            {
                counts[sourceId] = counts.GetValueOrDefault(sourceId) + n;
            }
        }

        if (counts.Count == 0)
        {
            return [];
        }

        var ids = counts.Keys.ToList();
        var rows = await (from s in db.Sources.AsNoTracking()
                          join l in db.Licenses.AsNoTracking() on s.DefaultLicenseId equals l.Id
                          where ids.Contains(s.Id)
                          orderby s.Name
                          select new { s, l }).ToListAsync(ct);

        return rows.Select(r => new AttributionItem(
            r.s.Id, r.s.Name, r.s.Provider, r.s.Url, r.l.Code, r.l.Name, r.l.Url, r.l.RequiresAttribution, r.l.ShareAlike, counts[r.s.Id])).ToList();
    }
}
