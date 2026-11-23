namespace Aduo.Core.Common.Security;

/// <summary>Tên vai trò. Endpoint KHÔNG kiểm tra vai trò, chỉ kiểm tra permission (docs phase-3/01 §4.3).</summary>
public static class Roles
{
    public const string Student = "Student";
    public const string Admin = "Admin";

    public static readonly IReadOnlyList<string> All = [Student, Admin];
}

/// <summary>
/// Bảng ánh xạ vai trò → permission, nằm trong code (không trong token, không trong DB).
/// Sau này tách vai trò Content Manager chỉ cần thêm một dòng ở đây.
/// </summary>
public static class RolePermissionMap
{
    private static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> Map =
        new Dictionary<string, IReadOnlyList<string>>
        {
            [Roles.Student] =
            [
                Permissions.LearningAccess,
                Permissions.AssessmentTake,
                Permissions.ReportsCreate,
            ],
            [Roles.Admin] = Permissions.All,
        };

    /// <summary>Gộp permission của mọi vai trò của người dùng.</summary>
    public static IReadOnlySet<string> For(IEnumerable<string> roles)
    {
        var result = new HashSet<string>(StringComparer.Ordinal);
        foreach (var role in roles)
        {
            if (Map.TryGetValue(role, out var permissions))
            {
                result.UnionWith(permissions);
            }
        }

        return result;
    }

    /// <summary>Permission chỉ được dùng khi email đã xác minh (BR-AUTH-07).</summary>
    public static bool RequiresVerifiedEmail(string permission)
        => permission is Permissions.AssessmentTake or Permissions.ReportsCreate;
}
