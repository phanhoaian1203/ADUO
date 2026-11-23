namespace Aduo.Core.Common.Content;

/// <summary>Vòng đời nội dung (BR-CNT-05). Chỉ <see cref="ContentLifecycle"/> được đổi trạng thái.</summary>
public enum ContentStatus
{
    Draft,
    InReview,
    Published,
    NeedsUpdate,
    Archived,
}

/// <summary>Nội dung được tạo ra bằng cách nào.</summary>
public enum ContentOrigin
{
    Imported,
    HumanAuthored,
    AIDraft,
    AIDraftHumanEdited,
}

/// <summary>Loại nội dung, dùng để lọc ở hàng đợi duyệt.</summary>
public enum ContentKind
{
    Vocabulary,
    ExampleSentence,
    Grammar,
    Topic,
    Lesson,
    Question,
    Article,
}

/// <summary>
/// Khối provenance + vòng đời dùng chung cho mọi bảng nội dung (docs/phase-3/02-database-design.md §1.1).
/// Map bằng EF Core complex type để các cột nằm ngay trong bảng nội dung (có FK thật tới sources/licenses).
/// <see cref="Status"/> có setter <c>internal</c> và chỉ <see cref="ContentLifecycle"/> được gọi nó
/// (architecture test <c>OnlyContentLifecycle_ChangesStatus</c> canh điều này), nên không có đường tắt bỏ qua PublishGuard.
/// </summary>
public sealed class ContentMeta
{
    public ContentStatus Status { get; internal set; } = ContentStatus.Draft;

    public Guid SourceId { get; set; }

    public short LicenseId { get; set; }

    public string? OriginalRef { get; set; }

    public ContentOrigin Origin { get; set; } = ContentOrigin.HumanAuthored;

    public string? AiModel { get; set; }

    public Guid? PromptTemplateId { get; set; }

    public Guid? ImportBatchId { get; set; }

    public Guid? ReviewedBy { get; set; }

    public DateTimeOffset? ReviewedAt { get; set; }

    public DateTimeOffset? PublishedAt { get; set; }

    /// <summary>Nội dung do AI tạo hoặc chỉnh một phần thì phải mang nhãn "Có hỗ trợ AI" (CR-003, BR-LEX-05).</summary>
    public bool AiAssisted => Origin is ContentOrigin.AIDraft or ContentOrigin.AIDraftHumanEdited;
}
