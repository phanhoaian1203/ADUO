using Aduo.Core.Common.Content;
using Aduo.Core.Common.Persistence;

namespace Aduo.Core.Modules.ContentOps.Domain;

/// <summary>Giấy phép. Dữ liệu cố định (seed qua migration), Admin không sửa cờ trong UI ở MVP.</summary>
public sealed class License
{
    public short Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Url { get; set; }

    /// <summary>Cho phép dùng thương mại. Nguồn có license này = false thì KHÔNG được import hay publish (BR-CNT-03).</summary>
    public bool AllowsCommercial { get; set; }

    public bool RequiresAttribution { get; set; }

    /// <summary>Share-alike: bản dịch/biến đổi phải giữ cùng giấy phép (BR-CNT-04).</summary>
    public bool ShareAlike { get; set; }

    public bool AllowsModification { get; set; }
}

public sealed class Source : IHasTimestamps
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public string Name { get; set; } = string.Empty;

    public string? Provider { get; set; }

    public string? Url { get; set; }

    public short DefaultLicenseId { get; set; }

    /// <summary>Ngày Admin đã đọc và xác minh giấy phép ở trang gốc. Null nghĩa là CHƯA xác minh (⚖️).</summary>
    public DateTimeOffset? LicenseVerifiedAt { get; set; }

    public string? Notes { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}

public enum MediaKind
{
    Audio,
    Image,
    ImportFile,
}

/// <summary>File đã lưu ở kho đối tượng (R2 hoặc thư mục local khi dev). Mỗi file có Source + License riêng (BR-CNT-11).</summary>
public sealed class MediaAsset : IHasTimestamps
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public MediaKind Kind { get; set; }

    public string Bucket { get; set; } = "default";

    /// <summary>Đường dẫn trong kho, do server tạo (UUID). KHÔNG BAO GIỜ dùng tên file người dùng gửi lên.</summary>
    public string StorageKey { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    /// <summary>Tên gốc để hiển thị, đã làm sạch. Không dùng để tạo đường dẫn.</summary>
    public string? OriginalFileName { get; set; }

    public long SizeBytes { get; set; }

    public int? DurationMs { get; set; }

    public string ChecksumSha256 { get; set; } = string.Empty;

    public string? OriginalUrl { get; set; }

    public string? Attribution { get; set; }

    public ContentMeta Meta { get; set; } = new();

    public Guid? UploadedBy { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}

public enum ImportBatchStatus
{
    DryRun,
    Committed,
    Cancelled,
}

public sealed class ImportBatch
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public string ContentType { get; set; } = string.Empty;

    public Guid SourceId { get; set; }

    public Guid FileMediaId { get; set; }

    public ImportBatchStatus Status { get; set; } = ImportBatchStatus.DryRun;

    /// <summary>Báo cáo dry-run dạng JSON (ImportReport). Cập nhật số liệu khi commit/hủy.</summary>
    public string Report { get; set; } = "{}";

    public Guid CreatedBy { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? CommittedAt { get; set; }
}

/// <summary>Lịch sử phiên bản. Bảng đa hình CÓ CHỦ ĐÍCH (không FK) vì chỉ là nhật ký (docs 02-database-design §3.7).</summary>
public sealed class ContentRevision
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public string EntityType { get; set; } = string.Empty;

    public Guid EntityId { get; set; }

    public int Version { get; set; }

    public string Snapshot { get; set; } = "{}";

    public Guid? ChangedBy { get; set; }

    public DateTimeOffset ChangedAt { get; set; }

    public string? ChangeNote { get; set; }
}

/// <summary>Bằng chứng đã duyệt theo checklist (BR-CNT-06): mỗi lần publish ghi một dòng cho TỪNG bản ghi.</summary>
public sealed class ReviewChecklistRecord
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public string EntityType { get; set; } = string.Empty;

    public Guid EntityId { get; set; }

    public int Version { get; set; }

    public string Items { get; set; } = "{}";

    public Guid ReviewedBy { get; set; }

    public DateTimeOffset ReviewedAt { get; set; }
}

/// <summary>Giá trị cấu hình của business rules (ngưỡng 60%, trần chi phí…). Đọc qua <c>ISettings</c>.</summary>
public sealed class AppSetting
{
    public string Key { get; set; } = string.Empty;

    /// <summary>Giá trị JSON (số, chuỗi…).</summary>
    public string Value { get; set; } = "null";

    public DateTimeOffset UpdatedAt { get; set; }
}
