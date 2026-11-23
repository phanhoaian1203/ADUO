namespace Aduo.Core.Common.Content;

/// <summary>Một dòng trong hàng đợi duyệt, gom từ mọi loại nội dung.</summary>
public sealed record ReviewQueueItem(
    string Resource,
    ContentKind Kind,
    Guid Id,
    string Title,
    ContentStatus Status,
    ContentOrigin Origin,
    Guid SourceId,
    int Version,
    DateTimeOffset UpdatedAt,
    IReadOnlyList<string> Flags)
{
    public bool Flagged => Flags.Count > 0 || Status == ContentStatus.NeedsUpdate;
}

public sealed record ReviewQueueQuery(ContentKind? Kind, ContentOrigin? Origin, Guid? SourceId);

/// <summary>
/// "Danh bạ" một loại nội dung với ContentOps. Mỗi module nội dung đăng ký một resource
/// (ví dụ "vocabulary-entries") để ContentOps dựng được API vòng đời chung, hàng đợi duyệt và trang ghi công.
/// Cài đặt mặc định cho entity EF: <c>EfContentResource&lt;T&gt;</c> (Infrastructure/Content).
/// </summary>
public interface IContentResource
{
    /// <summary>Đoạn URL: /admin/{resource}/{id}/publish.</summary>
    string Resource { get; }

    ContentKind Kind { get; }

    Task<IPublishable?> FindAsync(Guid id, CancellationToken ct);

    /// <summary>Bản ghi ở trạng thái Draft, InReview hoặc NeedsUpdate (chờ xử lý).</summary>
    Task<IReadOnlyList<ReviewQueueItem>> ListPendingAsync(ReviewQueueQuery query, CancellationToken ct);

    /// <summary>Số bản ghi Published theo từng nguồn, để dựng trang ghi công (FR-CMS-10).</summary>
    Task<IReadOnlyDictionary<Guid, int>> CountPublishedBySourceAsync(CancellationToken ct);

    /// <summary>Lưu thay đổi. Ném <see cref="ContentConcurrencyException"/> nếu bản ghi đã bị người khác sửa.</summary>
    Task SaveAsync(CancellationToken ct);
}

public sealed class ContentConcurrencyException(Exception? inner = null)
    : Exception("Nội dung đã bị người khác thay đổi.", inner);

/// <summary>Cho module nội dung gọi trước khi lưu một bản ghi ĐÃ Published đã được sửa (BR-CNT-09, docs 02-contentops §3.2).</summary>
public interface IContentVersioning
{
    /// <summary>Lưu bản cũ vào lịch sử rồi tăng <see cref="IPublishable.Version"/>. Gọi một lần mỗi lần sửa, TRƯỚC khi áp thay đổi.</summary>
    Task RecordEditAsync(string resource, IPublishable item, Guid changedBy, string? note, CancellationToken ct);
}
