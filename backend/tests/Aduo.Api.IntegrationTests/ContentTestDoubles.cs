using System.Collections.Concurrent;
using System.Text;
using Aduo.Core.Common.Content;
using Aduo.Core.Modules.ContentOps.Import;

namespace Aduo.Api.IntegrationTests;

/// <summary>
/// Loại nội dung giả (không có bảng DB) để kiểm thử API vòng đời chung, hàng đợi duyệt và trang ghi công
/// trước khi module Lexicon có entity thật. Đăng ký trong <see cref="ApiFactory"/>.
/// </summary>
public sealed class FakeItem : IPublishable
{
    public Guid Id { get; init; } = Guid.CreateVersion7();

    public ContentMeta Meta { get; } = new();

    public int Version { get; set; } = 1;

    public string Title { get; init; } = "item";

    public List<string> OwnReasons { get; } = [];

    public List<string> FlagList { get; } = [];

    public bool AiReviewed { get; set; }

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public IReadOnlyList<string> ValidateForPublish() => OwnReasons;

    public IReadOnlyList<string> Flags => FlagList;

    public bool HasAiReview => AiReviewed;

    public object ToSnapshot() => new { Title, Version };
}

public sealed class FakeContentResource : IContentResource
{
    private readonly ConcurrentDictionary<Guid, FakeItem> _items = new();

    public string Resource => "test-items";

    public ContentKind Kind => ContentKind.Vocabulary;

    public FakeItem Add(Guid sourceId, short licenseId, string title = "item", ContentOrigin origin = ContentOrigin.HumanAuthored)
    {
        var item = new FakeItem { Title = title };
        item.Meta.SourceId = sourceId;
        item.Meta.LicenseId = licenseId;
        item.Meta.Origin = origin;
        _items[item.Id] = item;
        return item;
    }

    public FakeItem Get(Guid id) => _items[id];

    public Task<IPublishable?> FindAsync(Guid id, CancellationToken ct)
        => Task.FromResult<IPublishable?>(_items.GetValueOrDefault(id));

    public Task<IReadOnlyList<ReviewQueueItem>> ListPendingAsync(ReviewQueueQuery q, CancellationToken ct)
    {
        IReadOnlyList<ReviewQueueItem> list = _items.Values
            .Where(i => i.Meta.Status is ContentStatus.Draft or ContentStatus.InReview or ContentStatus.NeedsUpdate)
            .Where(i => q.Origin is null || i.Meta.Origin == q.Origin)
            .Where(i => q.SourceId is null || i.Meta.SourceId == q.SourceId)
            .Select(i => new ReviewQueueItem(Resource, Kind, i.Id, i.Title, i.Meta.Status, i.Meta.Origin, i.Meta.SourceId, i.Version, i.UpdatedAt, i.Flags))
            .ToList();
        return Task.FromResult(list);
    }

    public Task<IReadOnlyDictionary<Guid, int>> CountPublishedBySourceAsync(CancellationToken ct)
        => Task.FromResult<IReadOnlyDictionary<Guid, int>>(_items.Values
            .Where(i => i.Meta.Status == ContentStatus.Published)
            .GroupBy(i => i.Meta.SourceId).ToDictionary(g => g.Key, g => g.Count()));

    public Task SaveAsync(CancellationToken ct) => Task.CompletedTask;
}

/// <summary>Một dòng của file import thử nghiệm: <c>key,name</c>.</summary>
public sealed record FakeRow(string Key, string Name);

public sealed record FakeStored(string Key, string Name, Guid BatchId, Guid SourceId, short LicenseId, bool Published);

/// <summary>Bộ xử lý import giả, giữ dữ liệu trong bộ nhớ. Kiểm thử khung dry-run/commit/hủy mà không cần bảng nội dung thật.</summary>
public sealed class FakeImportHandler : ImportHandler<FakeRow>
{
    public static readonly ConcurrentDictionary<string, FakeStored> Store = new();

    public override string ContentType => "test-rows";

    public override ImportTemplate GetTemplate(string format) => new("test-rows.csv", "text/csv", "key,name\nA1,Mutter\nA2,Vater\n");

    protected override IEnumerable<ParsedRow<FakeRow>> Parse(Stream file, string fileName)
    {
        using var reader = new StreamReader(file, Encoding.UTF8);
        _ = reader.ReadLine(); // tiêu đề
        var row = 0;
        while (reader.ReadLine() is { } line)
        {
            row++;
            var parts = line.Split(',');
            yield return parts.Length != 2
                ? new ParsedRow<FakeRow>(row, null, "Sai số cột")
                : new ParsedRow<FakeRow>(row, new FakeRow(parts[0].Trim(), parts[1].Trim()), null);
        }
    }

    protected override IReadOnlyList<ImportRowError> Validate(int row, FakeRow value)
        => string.IsNullOrWhiteSpace(value.Name) ? [new ImportRowError(row, "name", "Thiếu nghĩa")] : [];

    protected override string NaturalKey(FakeRow value) => "de|" + value.Key;

    protected override Task<IReadOnlyDictionary<string, string>> FindExistingAsync(IReadOnlyCollection<string> keys, CancellationToken ct)
        => Task.FromResult<IReadOnlyDictionary<string, string>>(keys.Where(Store.ContainsKey).ToDictionary(k => k, k => Store[k].Key));

    protected override Task InsertAsync(IReadOnlyList<FakeRow> rows, ImportCommitContext ctx, CancellationToken ct)
    {
        foreach (var r in rows)
        {
            Store["de|" + r.Key] = new FakeStored(r.Key, r.Name, ctx.BatchId, ctx.SourceId, ctx.LicenseId, Published: false);
        }

        return Task.CompletedTask;
    }

    public override Task<RollbackResult> RollbackAsync(Guid batchId, CancellationToken ct)
    {
        var mine = Store.Where(kv => kv.Value.BatchId == batchId).ToList();
        var kept = mine.Count(kv => kv.Value.Published);
        foreach (var kv in mine.Where(kv => !kv.Value.Published))
        {
            Store.TryRemove(kv.Key, out _);
        }

        return Task.FromResult(new RollbackResult(mine.Count - kept, kept));
    }
}
