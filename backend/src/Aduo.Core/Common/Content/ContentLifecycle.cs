using Aduo.Core.Common.Results;

namespace Aduo.Core.Common.Content;

/// <summary>
/// Một mẩu nội dung có thể đi qua quy trình duyệt. Mọi module nội dung (Lexicon, Assessment, InfoHub…) cài interface này
/// nên chỉ cần MỘT hàng đợi duyệt và MỘT API vòng đời cho tất cả (docs 02-contentops §3.3).
/// </summary>
public interface IPublishable
{
    Guid Id { get; }

    ContentMeta Meta { get; }

    /// <summary>Số phiên bản, cũng là concurrency token: hai tab cùng sửa thì tab sau nhận 409 (BR-CNT-09).</summary>
    int Version { get; set; }

    /// <summary>Tiêu đề ngắn hiển thị ở hàng đợi duyệt (ví dụ "die Mutter").</summary>
    string Title { get; }

    /// <summary>Điều kiện publish RIÊNG của loại nội dung (ví dụ danh từ phải có giống). Trả danh sách mã lỗi, rỗng nghĩa là đạt.</summary>
    IReadOnlyList<string> ValidateForPublish() => [];

    /// <summary>Cờ cảnh báo để Admin chú ý khi duyệt (auto-check lỗi, AI kiểm định có vấn đề…).</summary>
    IReadOnlyList<string> Flags => [];

    /// <summary>Đã có kết quả AI kiểm định chưa. Nội dung <c>AIDraft</c> bắt buộc có (BR-CNT-07).</summary>
    bool HasAiReview => false;

    /// <summary>Bản chụp để lưu lịch sử phiên bản. Phải là đối tượng tuần tự hóa được thành JSON, không chứa vòng tham chiếu.</summary>
    object ToSnapshot();
}

/// <summary>Checklist 6 mục khi duyệt (BR-CNT-06).</summary>
public sealed record PublishChecklist(
    bool SourceAndLicenseCorrect,
    bool ContentAccurate,
    bool LevelAppropriate,
    bool VietnameseNatural,
    bool MediaPlayable,
    bool FlagsReviewed)
{
    public bool AllChecked => SourceAndLicenseCorrect && ContentAccurate && LevelAppropriate
                              && VietnameseNatural && MediaPlayable && FlagsReviewed;
}

/// <summary>Thông tin ngoài bản ghi mà PublishGuard cần (nguồn, giấy phép, người duyệt, checklist).</summary>
public sealed record PublishContext(
    Guid ReviewerId,
    DateTimeOffset Now,
    bool SourceActive,
    bool LicenseAllowsCommercial,
    PublishChecklist? Checklist);

public static class ContentErrors
{
    public const string PublishCheckFailed = "PUBLISH_CHECK_FAILED";
    public const string InvalidTransition = "INVALID_TRANSITION";
    public const string NoteRequired = "NOTE_REQUIRED";

    public static Error PublishFailed(IReadOnlyList<string> reasons)
        => new(PublishCheckFailed, "Nội dung chưa đủ điều kiện để publish.", ErrorType.Rule)
        {
            FieldErrors = new Dictionary<string, string[]> { ["reasons"] = [.. reasons] },
        };

    public static Error Invalid(ContentStatus from, string action)
        => Error.Rule(InvalidTransition, $"Không thể {action} khi nội dung đang ở trạng thái {from}.");
}

/// <summary>
/// State machine duy nhất của vòng đời nội dung (BR-CNT-05). MỌI thay đổi <see cref="ContentMeta.Status"/> đi qua đây,
/// nên không có đường tắt bỏ qua <see cref="CheckPublishable"/>.
/// </summary>
public static class ContentLifecycle
{
    /// <summary>Draft → InReview.</summary>
    public static Result SubmitForReview(IPublishable item)
    {
        if (item.Meta.Status != ContentStatus.Draft)
        {
            return ContentErrors.Invalid(item.Meta.Status, "gửi duyệt");
        }

        item.Meta.Status = ContentStatus.InReview;
        return Result.Ok();
    }

    /// <summary>InReview → Draft. Bắt buộc ghi chú để người soạn biết cần sửa gì.</summary>
    public static Result ReturnToDraft(IPublishable item, string? note)
    {
        if (string.IsNullOrWhiteSpace(note))
        {
            return Error.Validation(ContentErrors.NoteRequired, "Hãy ghi chú lý do trả về nháp.");
        }

        if (item.Meta.Status != ContentStatus.InReview)
        {
            return ContentErrors.Invalid(item.Meta.Status, "trả về nháp");
        }

        item.Meta.Status = ContentStatus.Draft;
        return Result.Ok();
    }

    /// <summary>
    /// Kiểm tra điều kiện publish chung + riêng (BR-CNT-01, 03, 05, 06, 07). Không đổi trạng thái.
    /// Dùng được cả khi lưu một bản ghi đã Published (phải qua lại cổng này).
    /// </summary>
    public static Result CheckPublishable(IPublishable item, PublishContext ctx)
    {
        var reasons = new List<string>();

        if (item.Meta.SourceId == Guid.Empty || item.Meta.LicenseId == 0)
        {
            reasons.Add("SOURCE_OR_LICENSE_MISSING");
        }
        else
        {
            if (!ctx.SourceActive)
            {
                reasons.Add("SOURCE_INACTIVE");
            }

            if (!ctx.LicenseAllowsCommercial)
            {
                reasons.Add("LICENSE_NOT_COMMERCIAL");
            }
        }

        if (ctx.Checklist is not { AllChecked: true })
        {
            reasons.Add("CHECKLIST_INCOMPLETE");
        }

        if (item.Meta.Origin == ContentOrigin.AIDraft && !item.HasAiReview)
        {
            reasons.Add("AI_REVIEW_MISSING");
        }

        reasons.AddRange(item.ValidateForPublish());

        return reasons.Count == 0 ? Result.Ok() : ContentErrors.PublishFailed(reasons);
    }

    /// <summary>Draft | InReview | NeedsUpdate → Published. Ghi người duyệt và thời điểm.</summary>
    public static Result Publish(IPublishable item, PublishContext ctx)
    {
        if (item.Meta.Status is not (ContentStatus.Draft or ContentStatus.InReview or ContentStatus.NeedsUpdate))
        {
            return ContentErrors.Invalid(item.Meta.Status, "publish");
        }

        var check = CheckPublishable(item, ctx);
        if (!check.IsSuccess)
        {
            return check;
        }

        item.Meta.Status = ContentStatus.Published;
        item.Meta.ReviewedBy = ctx.ReviewerId;
        item.Meta.ReviewedAt = ctx.Now;
        item.Meta.PublishedAt ??= ctx.Now;
        return Result.Ok();
    }

    /// <summary>Published → NeedsUpdate (đủ báo lỗi, thống kê bất thường, bài Info Hub quá hạn).</summary>
    public static Result MarkNeedsUpdate(IPublishable item)
    {
        if (item.Meta.Status != ContentStatus.Published)
        {
            return ContentErrors.Invalid(item.Meta.Status, "đánh dấu cần cập nhật");
        }

        item.Meta.Status = ContentStatus.NeedsUpdate;
        return Result.Ok();
    }

    /// <summary>Published | NeedsUpdate → Archived. Nội dung đã Published không bị xóa cứng, chỉ lưu trữ.</summary>
    public static Result Archive(IPublishable item)
    {
        if (item.Meta.Status is not (ContentStatus.Published or ContentStatus.NeedsUpdate))
        {
            return ContentErrors.Invalid(item.Meta.Status, "lưu trữ");
        }

        item.Meta.Status = ContentStatus.Archived;
        return Result.Ok();
    }

    /// <summary>Chỉ được xóa cứng khi chưa từng Published (BR-CNT-12).</summary>
    public static bool CanHardDelete(IPublishable item)
        => item.Meta.Status == ContentStatus.Draft && item.Meta.PublishedAt is null;
}
