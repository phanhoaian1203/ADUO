namespace Aduo.Core.Common.Content;

/// <summary>Vòng đời nội dung (BR-CNT-05). Logic chuyển trạng thái nằm ở module ContentOps.</summary>
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

/// <summary>
/// Khối provenance + vòng đời dùng chung cho mọi bảng nội dung (docs/phase-3/02-database-design.md §1.1).
/// Map bằng EF Core complex type để các cột nằm ngay trong bảng nội dung (có FK thật tới sources/licenses).
/// Foundation chỉ khai báo. Quy tắc publish và chuyển trạng thái do ContentOps hiện thực.
/// </summary>
public sealed class ContentMeta
{
    public ContentStatus Status { get; set; } = ContentStatus.Draft;

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
}
