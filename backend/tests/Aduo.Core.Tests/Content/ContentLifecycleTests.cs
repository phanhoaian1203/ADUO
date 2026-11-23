using Aduo.Core.Common.Content;
using Aduo.Core.Common.Results;

namespace Aduo.Core.Tests.Content;

public class ContentLifecycleTests
{
    private sealed class Item : IPublishable
    {
        public Guid Id { get; } = Guid.CreateVersion7();

        public ContentMeta Meta { get; } = new() { SourceId = Guid.NewGuid(), LicenseId = 1, Origin = ContentOrigin.HumanAuthored };

        public int Version { get; set; } = 1;

        public string Title => "die Mutter";

        public List<string> Own { get; } = [];

        public bool AiReviewed { get; set; }

        public IReadOnlyList<string> ValidateForPublish() => Own;

        public bool HasAiReview => AiReviewed;

        public object ToSnapshot() => new { Title };
    }

    private static readonly PublishChecklist Full = new(true, true, true, true, true, true);

    private static PublishContext Ctx(PublishChecklist? list = null, bool commercial = true, bool active = true)
        => new(Guid.NewGuid(), DateTimeOffset.UtcNow, active, commercial, list ?? Full);

    private static string[] Reasons(Result r) => r.Error!.FieldErrors!["reasons"];

    [Fact]
    public void FullHappyPath_DraftToInReviewToPublishedToArchived()
    {
        var item = new Item();

        Assert.True(ContentLifecycle.SubmitForReview(item).IsSuccess);
        Assert.Equal(ContentStatus.InReview, item.Meta.Status);
        Assert.True(ContentLifecycle.Publish(item, Ctx()).IsSuccess);
        Assert.Equal(ContentStatus.Published, item.Meta.Status);
        Assert.NotNull(item.Meta.PublishedAt);
        Assert.NotNull(item.Meta.ReviewedBy);
        Assert.True(ContentLifecycle.Archive(item).IsSuccess);
        Assert.Equal(ContentStatus.Archived, item.Meta.Status);
    }

    [Fact]
    public void Publish_FromDraft_IsAllowed()
    {
        var item = new Item();

        Assert.True(ContentLifecycle.Publish(item, Ctx()).IsSuccess);
    }

    [Fact]
    public void Publish_WithIncompleteChecklist_Fails() // CO-T06
    {
        var item = new Item();

        var result = ContentLifecycle.Publish(item, Ctx(Full with { MediaPlayable = false }));

        Assert.Equal(ContentErrors.PublishCheckFailed, result.Error!.Code);
        Assert.Contains("CHECKLIST_INCOMPLETE", Reasons(result));
        Assert.Equal(ContentStatus.Draft, item.Meta.Status);
    }

    [Fact]
    public void Publish_WithoutChecklist_Fails()
    {
        var result = ContentLifecycle.Publish(new Item(), new PublishContext(Guid.NewGuid(), DateTimeOffset.UtcNow, true, true, null));

        Assert.Contains("CHECKLIST_INCOMPLETE", Reasons(result));
    }

    [Fact]
    public void Publish_AiDraftWithoutAiReview_Fails() // CO-T07
    {
        var item = new Item();
        item.Meta.Origin = ContentOrigin.AIDraft;

        var result = ContentLifecycle.Publish(item, Ctx());

        Assert.Contains("AI_REVIEW_MISSING", Reasons(result));

        item.AiReviewed = true;
        Assert.True(ContentLifecycle.Publish(item, Ctx()).IsSuccess);
    }

    [Fact]
    public void Publish_WithNonCommercialLicense_Fails() // BR-CNT-03
    {
        var result = ContentLifecycle.Publish(new Item(), Ctx(commercial: false));

        Assert.Contains("LICENSE_NOT_COMMERCIAL", Reasons(result));
    }

    [Fact]
    public void Publish_WithInactiveSource_Fails()
    {
        var result = ContentLifecycle.Publish(new Item(), Ctx(active: false));

        Assert.Contains("SOURCE_INACTIVE", Reasons(result));
    }

    [Fact]
    public void Publish_WithoutSourceOrLicense_Fails() // BR-CNT-01
    {
        var item = new Item();
        item.Meta.SourceId = Guid.Empty;

        Assert.Contains("SOURCE_OR_LICENSE_MISSING", Reasons(ContentLifecycle.Publish(item, Ctx())));
    }

    [Fact]
    public void Publish_CollectsAllReasonsIncludingModuleSpecificOnes()
    {
        var item = new Item();
        item.Own.Add("MISSING_GENDER");

        var result = ContentLifecycle.Publish(item, Ctx(Full with { ContentAccurate = false }, commercial: false));

        Assert.Equal(["LICENSE_NOT_COMMERCIAL", "CHECKLIST_INCOMPLETE", "MISSING_GENDER"], Reasons(result));
    }

    [Fact]
    public void ReturnToDraft_WithoutNote_ReturnsValidationError() // CO-T08
    {
        var item = new Item();
        ContentLifecycle.SubmitForReview(item);

        var result = ContentLifecycle.ReturnToDraft(item, "  ");

        Assert.Equal(ErrorType.Validation, result.Error!.Type);
        Assert.Equal(ContentStatus.InReview, item.Meta.Status);
    }

    [Fact]
    public void ReturnToDraft_WithNote_Works()
    {
        var item = new Item();
        ContentLifecycle.SubmitForReview(item);

        Assert.True(ContentLifecycle.ReturnToDraft(item, "Sửa nghĩa").IsSuccess);
        Assert.Equal(ContentStatus.Draft, item.Meta.Status);
    }

    [Theory]
    [InlineData(ContentStatus.Published)]
    [InlineData(ContentStatus.Archived)]
    [InlineData(ContentStatus.NeedsUpdate)]
    [InlineData(ContentStatus.Draft)]
    public void SubmitForReview_OnlyFromDraft(ContentStatus from)
    {
        var item = new Item();
        MoveTo(item, from);

        var result = ContentLifecycle.SubmitForReview(item);

        Assert.Equal(from == ContentStatus.Draft, result.IsSuccess);
    }

    [Fact]
    public void MarkNeedsUpdate_OnlyFromPublished_ThenCanRepublish()
    {
        var item = new Item();
        Assert.False(ContentLifecycle.MarkNeedsUpdate(item).IsSuccess);

        ContentLifecycle.Publish(item, Ctx());
        Assert.True(ContentLifecycle.MarkNeedsUpdate(item).IsSuccess);
        Assert.Equal(ContentStatus.NeedsUpdate, item.Meta.Status);
        Assert.True(ContentLifecycle.Publish(item, Ctx()).IsSuccess);
    }

    [Fact]
    public void Archive_FromDraft_IsRejected()
    {
        Assert.False(ContentLifecycle.Archive(new Item()).IsSuccess);
    }

    [Fact]
    public void Publish_KeepsOriginalPublishedAt_OnRepublish()
    {
        var item = new Item();
        var first = DateTimeOffset.UtcNow.AddDays(-3);
        ContentLifecycle.Publish(item, Ctx() with { Now = first });
        ContentLifecycle.MarkNeedsUpdate(item);

        ContentLifecycle.Publish(item, Ctx() with { Now = first.AddDays(3) });

        Assert.Equal(first, item.Meta.PublishedAt);
    }

    [Fact]
    public void HardDelete_OnlyWhenNeverPublished() // BR-CNT-12
    {
        var item = new Item();
        Assert.True(ContentLifecycle.CanHardDelete(item));

        ContentLifecycle.Publish(item, Ctx());
        ContentLifecycle.MarkNeedsUpdate(item);
        Assert.False(ContentLifecycle.CanHardDelete(item));
    }

    [Fact]
    public void AiAssisted_TrueForAiOrigins_FalseOtherwise() // CO-T21, CO-T22
    {
        foreach (var (origin, expected) in new[]
                 {
                     (ContentOrigin.AIDraft, true), (ContentOrigin.AIDraftHumanEdited, true),
                     (ContentOrigin.HumanAuthored, false), (ContentOrigin.Imported, false),
                 })
        {
            Assert.Equal(expected, new ContentMeta { Origin = origin }.AiAssisted);
        }
    }

    // Đi từng bước hợp lệ qua đúng cổng duy nhất, nên không cần mở khóa setter internal của Status.
    private static void MoveTo(Item item, ContentStatus target)
    {
        switch (target)
        {
            case ContentStatus.InReview:
                ContentLifecycle.SubmitForReview(item);
                break;
            case ContentStatus.Published:
                ContentLifecycle.Publish(item, Ctx());
                break;
            case ContentStatus.NeedsUpdate:
                ContentLifecycle.Publish(item, Ctx());
                ContentLifecycle.MarkNeedsUpdate(item);
                break;
            case ContentStatus.Archived:
                ContentLifecycle.Publish(item, Ctx());
                ContentLifecycle.Archive(item);
                break;
        }
    }
}
