namespace Aduo.Core.Common.Security;

/// <summary>
/// Danh sách quyền (docs/phase-3/01-system-architecture.md §4.3).
/// Endpoint yêu cầu QUYỀN, không yêu cầu TÊN VAI TRÒ. Bảng ánh xạ vai trò → quyền được thêm ở module Identity.
/// </summary>
public static class Permissions
{
    public const string LearningAccess = "learning.access";
    public const string AssessmentTake = "assessment.take";
    public const string ReportsCreate = "reports.create";
    public const string ContentManage = "content.manage";
    public const string ContentPublish = "content.publish";
    public const string InfoHubManage = "infohub.manage";
    public const string ReportsManage = "reports.manage";
    public const string UsersManage = "users.manage";

    public static readonly IReadOnlyList<string> All =
    [
        LearningAccess, AssessmentTake, ReportsCreate, ContentManage,
        ContentPublish, InfoHubManage, ReportsManage, UsersManage,
    ];
}
