namespace Aduo.Core.Modules.ContentOps.Import;

public sealed record ImportRowError(int Row, string Field, string Message);

/// <param name="Row">Số dòng trong file (bắt đầu từ 1, không tính dòng tiêu đề).</param>
/// <param name="Key">Khóa tự nhiên, ví dụ "de|Haus|Noun|1".</param>
/// <param name="ExistingId">Id bản ghi đã có trong DB. Null nếu trùng ngay trong file.</param>
/// <param name="Where">"database" hoặc "file".</param>
public sealed record ImportDuplicate(int Row, string Key, string? ExistingId, string Where);

public sealed class ImportReport
{
    public int Total { get; set; }

    /// <summary>Số dòng hợp lệ VÀ chưa có (sẽ được tạo khi commit). Không gồm dòng lỗi hay dòng trùng.</summary>
    public int Valid { get; set; }

    public List<ImportRowError> Errors { get; set; } = [];

    public List<ImportDuplicate> Duplicates { get; set; } = [];

    /// <summary>Số bản ghi thực sự đã tạo khi commit.</summary>
    public int? Inserted { get; set; }

    /// <summary>Khi hủy batch: số bản ghi đã xóa và số bản ghi đã Published được giữ lại (BR-CNT-12).</summary>
    public int? Deleted { get; set; }

    public int? KeptPublished { get; set; }
}

public sealed record ImportTemplate(string FileName, string ContentType, string Content);

public sealed record ParsedRow<TRow>(int Row, TRow? Value, string? ParseError);

/// <summary>Thông tin gắn vào mọi bản ghi tạo ra từ một batch (provenance).</summary>
public sealed record ImportCommitContext(Guid BatchId, Guid SourceId, short LicenseId, Guid UserId);

public sealed record RollbackResult(int Deleted, int KeptPublished);

/// <summary>
/// Bộ xử lý import cho MỘT loại nội dung (ví dụ từ vựng). ContentOps cung cấp khung (dry-run, commit, hủy, provenance);
/// mỗi module nội dung cài đặt bộ xử lý của mình, tốt nhất bằng cách kế thừa <see cref="ImportHandler{TRow}"/>.
/// CMS và CLI dùng CHUNG bộ xử lý này (docs 02-contentops §4.5).
/// </summary>
public interface IImportHandler
{
    string ContentType { get; }

    ImportTemplate GetTemplate(string format);

    /// <summary>Đọc, kiểm tra, loại trùng và trả báo cáo. KHÔNG ghi gì vào DB (dry-run).</summary>
    Task<ImportReport> AnalyzeAsync(Stream file, string fileName, CancellationToken ct);

    /// <summary>Đọc lại file, rồi tạo bản ghi Draft cho các dòng hợp lệ và chưa có. Trả số bản ghi đã tạo. Chạy trong transaction của khung.</summary>
    Task<int> CommitAsync(Stream file, string fileName, ImportCommitContext ctx, CancellationToken ct);

    /// <summary>Xóa các bản ghi của batch chưa từng Published. Giữ lại và đếm các bản ghi đã Published.</summary>
    Task<RollbackResult> RollbackAsync(Guid batchId, CancellationToken ct);
}

/// <summary>
/// Pipeline chung: Parse → Validate → Dedup (trong file, rồi so với DB) → báo cáo. Tách ba bước rõ ràng để chạy thử được và làm lại được.
/// Idempotent: import lại cùng file thì mọi dòng đã hợp lệ đều thành "trùng", không sinh bản ghi lặp (CO-T04).
/// </summary>
public abstract class ImportHandler<TRow> : IImportHandler
    where TRow : class
{
    public abstract string ContentType { get; }

    public abstract ImportTemplate GetTemplate(string format);

    protected abstract IEnumerable<ParsedRow<TRow>> Parse(Stream file, string fileName);

    protected abstract IReadOnlyList<ImportRowError> Validate(int row, TRow value);

    protected abstract string NaturalKey(TRow value);

    /// <summary>Trong các khóa này, khóa nào đã có trong DB? Trả khóa → id bản ghi hiện có.</summary>
    protected abstract Task<IReadOnlyDictionary<string, string>> FindExistingAsync(IReadOnlyCollection<string> keys, CancellationToken ct);

    /// <summary>Tạo bản ghi Draft có đủ provenance (source, license, import_batch_id, origin=Imported).</summary>
    protected abstract Task InsertAsync(IReadOnlyList<TRow> rows, ImportCommitContext ctx, CancellationToken ct);

    public abstract Task<RollbackResult> RollbackAsync(Guid batchId, CancellationToken ct);

    public async Task<ImportReport> AnalyzeAsync(Stream file, string fileName, CancellationToken ct)
        => (await AnalyzeCoreAsync(file, fileName, ct)).Report;

    public async Task<int> CommitAsync(Stream file, string fileName, ImportCommitContext ctx, CancellationToken ct)
    {
        var analysis = await AnalyzeCoreAsync(file, fileName, ct);
        if (analysis.Insertable.Count > 0)
        {
            await InsertAsync(analysis.Insertable, ctx, ct);
        }

        return analysis.Insertable.Count;
    }

    private async Task<(ImportReport Report, List<TRow> Insertable)> AnalyzeCoreAsync(Stream file, string fileName, CancellationToken ct)
    {
        var report = new ImportReport();
        var candidates = new List<(int Row, string Key, TRow Value)>();
        var seen = new HashSet<string>(StringComparer.Ordinal);

        foreach (var parsed in Parse(file, fileName))
        {
            ct.ThrowIfCancellationRequested();
            report.Total++;

            if (parsed.ParseError is not null || parsed.Value is null)
            {
                report.Errors.Add(new ImportRowError(parsed.Row, "", parsed.ParseError ?? "Không đọc được dòng."));
                continue;
            }

            var errors = Validate(parsed.Row, parsed.Value);
            if (errors.Count > 0)
            {
                report.Errors.AddRange(errors);
                continue;
            }

            var key = NaturalKey(parsed.Value);
            if (!seen.Add(key))
            {
                report.Duplicates.Add(new ImportDuplicate(parsed.Row, key, null, "file"));
                continue;
            }

            candidates.Add((parsed.Row, key, parsed.Value));
        }

        var existing = candidates.Count == 0
            ? new Dictionary<string, string>()
            : (await FindExistingAsync(candidates.Select(c => c.Key).ToList(), ct)).ToDictionary(kv => kv.Key, kv => kv.Value);

        var insertable = new List<TRow>();
        foreach (var (row, key, value) in candidates)
        {
            if (existing.TryGetValue(key, out var existingId))
            {
                report.Duplicates.Add(new ImportDuplicate(row, key, existingId, "database"));
            }
            else
            {
                insertable.Add(value);
            }
        }

        report.Valid = insertable.Count;
        return (report, insertable);
    }
}
