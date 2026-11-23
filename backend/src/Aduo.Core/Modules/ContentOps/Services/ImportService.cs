using System.Text.Json;
using Aduo.Core.Common.Results;
using Aduo.Core.Infrastructure.Persistence;
using Aduo.Core.Infrastructure.Storage;
using Aduo.Core.Modules.ContentOps.Domain;
using Aduo.Core.Modules.ContentOps.Import;
using Microsoft.EntityFrameworkCore;

namespace Aduo.Core.Modules.ContentOps.Services;

public sealed record ImportBatchResponse(
    Guid Id, string ContentType, Guid SourceId, ImportBatchStatus Status, ImportReport Report, DateTimeOffset CreatedAt, DateTimeOffset? CommittedAt);

/// <summary>
/// Điều phối vòng đời một batch import: dry-run → commit → (tùy chọn) hủy (docs 02-contentops §4.5).
/// Commit chạy trong một transaction; hai request commit đồng thời chỉ một bên "giành" được batch.
/// </summary>
public sealed class ImportService(
    AppDbContext db,
    MediaService media,
    IObjectStorage storage,
    IEnumerable<IImportHandler> handlers,
    TimeProvider time)
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public static readonly Error UnknownContentType = Error.Validation("UNKNOWN_CONTENT_TYPE", "Loại nội dung này chưa hỗ trợ import.");
    public static readonly Error BatchNotFound = Error.NotFound("Không tìm thấy batch import.");
    public static readonly Error BatchNotDryRun = Error.Conflict("BATCH_NOT_DRY_RUN", "Batch này không còn ở trạng thái chờ xác nhận.");
    public static readonly Error BatchAlreadyCancelled = Error.Conflict("BATCH_ALREADY_CANCELLED", "Batch này đã được hủy.");

    public IImportHandler? Find(string contentType)
        => handlers.FirstOrDefault(h => string.Equals(h.ContentType, contentType, StringComparison.OrdinalIgnoreCase));

    public IReadOnlyList<string> ContentTypes => handlers.Select(h => h.ContentType).Order().ToList();

    public async Task<Result<ImportBatchResponse>> DryRunAsync(
        string contentType, Guid sourceId, string? fileName, Stream content, Guid userId, CancellationToken ct)
    {
        var handler = Find(contentType);
        if (handler is null)
        {
            return UnknownContentType;
        }

        // Kiểm tra giấy phép TRƯỚC KHI lưu file hay tạo batch (CO-T01: không tạo batch).
        var source = await db.Sources.AsNoTracking().SingleOrDefaultAsync(s => s.Id == sourceId, ct);
        if (source is null)
        {
            return ContentOpsErrors.SourceNotFound;
        }

        if (!source.IsActive)
        {
            return ContentOpsErrors.SourceInactive;
        }

        var license = await db.Licenses.AsNoTracking().SingleAsync(l => l.Id == source.DefaultLicenseId, ct);
        if (!license.AllowsCommercial)
        {
            return ContentOpsErrors.LicenseNotCommercial(license.Code);
        }

        var uploaded = await media.UploadAsync(
            new UploadMediaRequest(fileName, content, sourceId, null, null, null, userId, MediaKind.ImportFile), ct);
        if (!uploaded.IsSuccess)
        {
            return uploaded.Error;
        }

        var file = uploaded.Value.Media;
        await using var stream = await storage.OpenReadAsync(file.StorageKey, ct)
                                 ?? throw new InvalidOperationException("File import vừa lưu không đọc lại được.");
        var report = await handler.AnalyzeAsync(stream, file.OriginalFileName ?? "import" + Path.GetExtension(file.StorageKey), ct);

        var batch = new ImportBatch
        {
            ContentType = handler.ContentType,
            SourceId = sourceId,
            FileMediaId = file.Id,
            Status = ImportBatchStatus.DryRun,
            Report = JsonSerializer.Serialize(report, Json),
            CreatedBy = userId,
            CreatedAt = time.GetUtcNow(),
        };
        db.ImportBatches.Add(batch);
        await db.SaveChangesAsync(ct);

        return ToResponse(batch, report);
    }

    public async Task<Result<ImportBatchResponse>> CommitAsync(Guid batchId, Guid userId, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(ct);

        // "Giành" batch bằng một câu UPDATE có điều kiện: chỉ một request đồng thời thành công.
        var now = time.GetUtcNow();
        var claimed = await db.ImportBatches
            .Where(b => b.Id == batchId && b.Status == ImportBatchStatus.DryRun)
            .ExecuteUpdateAsync(s => s.SetProperty(b => b.Status, ImportBatchStatus.Committed).SetProperty(b => b.CommittedAt, now), ct);
        if (claimed == 0)
        {
            return await db.ImportBatches.AnyAsync(b => b.Id == batchId, ct) ? BatchNotDryRun : BatchNotFound;
        }

        var batch = await db.ImportBatches.AsNoTracking().SingleAsync(b => b.Id == batchId, ct);
        var handler = Find(batch.ContentType) ?? throw new InvalidOperationException($"Thiếu bộ xử lý import cho {batch.ContentType}.");
        var source = await db.Sources.AsNoTracking().SingleAsync(s => s.Id == batch.SourceId, ct);
        var file = await db.MediaAssets.AsNoTracking().SingleAsync(m => m.Id == batch.FileMediaId, ct);

        await using var stream = await storage.OpenReadAsync(file.StorageKey, ct)
                                 ?? throw new InvalidOperationException("Không đọc lại được file import.");
        var ctx = new ImportCommitContext(batch.Id, source.Id, source.DefaultLicenseId, userId);
        var inserted = await handler.CommitAsync(stream, file.OriginalFileName ?? "import" + Path.GetExtension(file.StorageKey), ctx, ct);

        // Phân tích lại lúc commit nên báo cáo cuối cùng phản ánh đúng trạng thái DB tại thời điểm commit.
        var report = JsonSerializer.Deserialize<ImportReport>(batch.Report, Json) ?? new ImportReport();
        report.Inserted = inserted;
        var json = JsonSerializer.Serialize(report, Json);
        await db.ImportBatches.Where(b => b.Id == batchId).ExecuteUpdateAsync(s => s.SetProperty(b => b.Report, json), ct);

        await tx.CommitAsync(ct);
        return ToResponse(batch, report);
    }

    public async Task<Result<ImportBatchResponse>> CancelAsync(Guid batchId, CancellationToken ct)
    {
        var batch = await db.ImportBatches.SingleOrDefaultAsync(b => b.Id == batchId, ct);
        if (batch is null)
        {
            return BatchNotFound;
        }

        if (batch.Status == ImportBatchStatus.Cancelled)
        {
            return BatchAlreadyCancelled;
        }

        var report = JsonSerializer.Deserialize<ImportReport>(batch.Report, Json) ?? new ImportReport();
        if (batch.Status == ImportBatchStatus.Committed)
        {
            var handler = Find(batch.ContentType) ?? throw new InvalidOperationException($"Thiếu bộ xử lý import cho {batch.ContentType}.");
            var rolled = await handler.RollbackAsync(batch.Id, ct);
            report.Deleted = rolled.Deleted;
            report.KeptPublished = rolled.KeptPublished;
        }

        batch.Status = ImportBatchStatus.Cancelled;
        batch.Report = JsonSerializer.Serialize(report, Json);
        await db.SaveChangesAsync(ct);
        return ToResponse(batch, report);
    }

    public async Task<Result<ImportBatchResponse>> GetAsync(Guid batchId, CancellationToken ct)
    {
        var batch = await db.ImportBatches.AsNoTracking().SingleOrDefaultAsync(b => b.Id == batchId, ct);
        return batch is null ? BatchNotFound : ToResponse(batch, JsonSerializer.Deserialize<ImportReport>(batch.Report, Json) ?? new ImportReport());
    }

    public async Task<IReadOnlyList<ImportBatchResponse>> ListAsync(int page, int pageSize, CancellationToken ct)
    {
        var rows = await db.ImportBatches.AsNoTracking()
            .OrderByDescending(b => b.CreatedAt)
            .Skip((Math.Max(page, 1) - 1) * pageSize).Take(pageSize)
            .ToListAsync(ct);
        return rows.Select(b => ToResponse(b, JsonSerializer.Deserialize<ImportReport>(b.Report, Json) ?? new ImportReport())).ToList();
    }

    private static ImportBatchResponse ToResponse(ImportBatch b, ImportReport report)
        => new(b.Id, b.ContentType, b.SourceId, b.Status, report, b.CreatedAt, b.CommittedAt);
}
