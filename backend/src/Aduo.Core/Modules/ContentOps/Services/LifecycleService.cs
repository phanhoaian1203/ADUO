using System.Text.Json;
using Aduo.Core.Common.Content;
using Aduo.Core.Common.Results;
using Aduo.Core.Infrastructure.Persistence;
using Aduo.Core.Modules.ContentOps.Domain;
using Microsoft.EntityFrameworkCore;

namespace Aduo.Core.Modules.ContentOps.Services;

public sealed record ChecklistRequest(
    bool SourceAndLicenseCorrect,
    bool ContentAccurate,
    bool LevelAppropriate,
    bool VietnameseNatural,
    bool MediaPlayable,
    bool FlagsReviewed)
{
    public PublishChecklist ToChecklist()
        => new(SourceAndLicenseCorrect, ContentAccurate, LevelAppropriate, VietnameseNatural, MediaPlayable, FlagsReviewed);
}

public sealed record ContentActionResponse(string Resource, Guid Id, ContentStatus Status, int Version);

public static class LifecycleErrors
{
    public static Error UnknownResource(string resource) => Error.NotFound($"Không có loại nội dung '{resource}'.");

    public static readonly Error ItemNotFound = Error.NotFound("Không tìm thấy nội dung.");

    public static readonly Error Concurrency = Error.Conflict(ErrorCodes.ConcurrencyConflict, "Nội dung đã bị người khác thay đổi. Hãy tải lại rồi thử lại.");
}

/// <summary>
/// API vòng đời CHUNG cho mọi loại nội dung (submit-review, publish, return-to-draft, archive).
/// Mọi phép chuyển trạng thái đi qua <see cref="ContentLifecycle"/>; lớp này lo phần ngoài state machine:
/// khóa lạc quan, nạp nguồn/giấy phép cho PublishGuard, ghi checklist và lịch sử phiên bản.
/// </summary>
public sealed class LifecycleService(AppDbContext db, IEnumerable<IContentResource> resources, TimeProvider time)
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public IContentResource? FindResource(string resource)
        => resources.FirstOrDefault(r => string.Equals(r.Resource, resource, StringComparison.OrdinalIgnoreCase));

    public IReadOnlyList<IContentResource> All => resources.ToList();

    public Task<Result<ContentActionResponse>> SubmitReviewAsync(string resource, Guid id, int version, CancellationToken ct)
        => ApplyAsync(resource, id, version, ContentLifecycle.SubmitForReview, ct);

    public Task<Result<ContentActionResponse>> ReturnToDraftAsync(string resource, Guid id, int version, string? note, CancellationToken ct)
        => ApplyAsync(resource, id, version, item => ContentLifecycle.ReturnToDraft(item, note), ct);

    public Task<Result<ContentActionResponse>> ArchiveAsync(string resource, Guid id, int version, CancellationToken ct)
        => ApplyAsync(resource, id, version, ContentLifecycle.Archive, ct);

    public async Task<Result<ContentActionResponse>> PublishAsync(
        string resource, Guid id, int version, ChecklistRequest? checklist, Guid reviewerId, CancellationToken ct)
    {
        var loaded = await LoadAsync(resource, id, version, ct);
        if (!loaded.IsSuccess)
        {
            return loaded.Error;
        }

        var (res, item) = loaded.Value;
        var now = time.GetUtcNow();

        var source = await db.Sources.AsNoTracking().SingleOrDefaultAsync(s => s.Id == item.Meta.SourceId, ct);
        var license = await db.Licenses.AsNoTracking().SingleOrDefaultAsync(l => l.Id == item.Meta.LicenseId, ct);
        var ctx = new PublishContext(
            reviewerId, now,
            SourceActive: source?.IsActive ?? false,
            // Thiếu source/license thì để PublishGuard báo SOURCE_OR_LICENSE_MISSING; ở đây coi là không thương mại cho an toàn.
            LicenseAllowsCommercial: license?.AllowsCommercial ?? false,
            Checklist: checklist?.ToChecklist());

        var result = ContentLifecycle.Publish(item, ctx);
        if (!result.IsSuccess)
        {
            return result.Error;
        }

        // Bằng chứng duyệt (BR-CNT-06) và lịch sử phiên bản (BR-CNT-09) cho TỪNG bản ghi, cùng một lần lưu với trạng thái mới.
        db.ReviewChecklists.Add(new ReviewChecklistRecord
        {
            EntityType = res.Resource,
            EntityId = item.Id,
            Version = item.Version,
            Items = JsonSerializer.Serialize(checklist, Json),
            ReviewedBy = reviewerId,
            ReviewedAt = now,
        });
        db.ContentRevisions.Add(NewRevision(res.Resource, item, reviewerId, "Publish", now));

        return await SaveAsync(res, item, ct);
    }

    private async Task<Result<ContentActionResponse>> ApplyAsync(
        string resource, Guid id, int version, Func<IPublishable, Result> action, CancellationToken ct)
    {
        var loaded = await LoadAsync(resource, id, version, ct);
        if (!loaded.IsSuccess)
        {
            return loaded.Error;
        }

        var (res, item) = loaded.Value;
        var result = action(item);
        return result.IsSuccess ? await SaveAsync(res, item, ct) : result.Error;
    }

    private async Task<Result<(IContentResource Res, IPublishable Item)>> LoadAsync(string resource, Guid id, int version, CancellationToken ct)
    {
        var res = FindResource(resource);
        if (res is null)
        {
            return LifecycleErrors.UnknownResource(resource);
        }

        var item = await res.FindAsync(id, ct);
        if (item is null)
        {
            return LifecycleErrors.ItemNotFound;
        }

        // Khóa lạc quan (CO-T09): client gửi version đang thấy; lệch nghĩa là có người đã sửa nội dung này.
        if (item.Version != version)
        {
            return LifecycleErrors.Concurrency;
        }

        return (res, item);
    }

    private async Task<Result<ContentActionResponse>> SaveAsync(IContentResource res, IPublishable item, CancellationToken ct)
    {
        try
        {
            await res.SaveAsync(ct);
            // Checklist và lịch sử phiên bản nằm trong cùng AppDbContext. Resource dùng EF đã lưu chúng cùng lúc; gọi thêm để chắc chắn
            // (và cho resource không dùng EF). Không có gì để lưu thì không tốn gì.
            await db.SaveChangesAsync(ct);
        }
        catch (ContentConcurrencyException)
        {
            return LifecycleErrors.Concurrency;
        }

        return new ContentActionResponse(res.Resource, item.Id, item.Meta.Status, item.Version);
    }

    internal static ContentRevision NewRevision(string resource, IPublishable item, Guid? by, string? note, DateTimeOffset now)
        => new()
        {
            EntityType = resource,
            EntityId = item.Id,
            Version = item.Version,
            Snapshot = JsonSerializer.Serialize(item.ToSnapshot(), Json),
            ChangedBy = by,
            ChangedAt = now,
            ChangeNote = note,
        };
}

/// <summary>Lưu bản cũ vào lịch sử rồi tăng version khi sửa nội dung đã Published (BR-CNT-09, CO-T10).</summary>
public sealed class ContentVersioning(AppDbContext db, TimeProvider time) : IContentVersioning
{
    public Task RecordEditAsync(string resource, IPublishable item, Guid changedBy, string? note, CancellationToken ct)
    {
        // Bản nháp chưa từng publish sửa thoải mái, không cần lịch sử. Chỉ nội dung đang phục vụ người học mới cần.
        if (item.Meta.Status is ContentStatus.Published or ContentStatus.NeedsUpdate)
        {
            db.ContentRevisions.Add(LifecycleService.NewRevision(resource, item, changedBy, note, time.GetUtcNow()));
            item.Version++;
        }

        return Task.CompletedTask;
    }
}
