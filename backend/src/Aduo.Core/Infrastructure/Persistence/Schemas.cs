namespace Aduo.Core.Infrastructure.Persistence;

/// <summary>Một schema PostgreSQL cho mỗi module (AR-03). Module chỉ được truy cập bảng trong schema của mình.</summary>
public static class Schemas
{
    public const string Identity = "identity";
    public const string Catalog = "catalog";
    public const string Lexicon = "lexicon";
    public const string Assessment = "assessment";
    public const string Progress = "progress";
    public const string InfoHub = "infohub";
    public const string Content = "content";

    public static readonly IReadOnlyList<string> All = [Identity, Catalog, Lexicon, Assessment, Progress, InfoHub, Content];
}
