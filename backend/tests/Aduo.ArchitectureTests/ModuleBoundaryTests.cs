using NetArchTest.Rules;

namespace Aduo.ArchitectureTests;

/// <summary>
/// Kiểm tra ranh giới kiến trúc (AR-03). Test này chạy trong CI, nên vi phạm sẽ bị phát hiện ngay khi push.
/// Khi các module được thêm vào, bổ sung luật "module A không phụ thuộc phần nội bộ của module B".
/// </summary>
public class ModuleBoundaryTests
{
    private static readonly System.Reflection.Assembly Core = typeof(Aduo.Core.DependencyInjection).Assembly;

    [Fact]
    public void Core_DoesNotDependOn_Api() // F-T10
    {
        var result = Types.InAssembly(Core).ShouldNot().HaveDependencyOn("Aduo.Api").GetResult();
        Assert.True(result.IsSuccessful, Describe(result));
    }

    [Fact]
    public void Core_DoesNotDependOn_Cli()
    {
        var result = Types.InAssembly(Core).ShouldNot().HaveDependencyOn("Aduo.Cli").GetResult();
        Assert.True(result.IsSuccessful, Describe(result));
    }

    [Fact]
    public void Common_DoesNotDependOn_Modules()
    {
        // Common là phần dùng chung, không được biết tới module nghiệp vụ nào.
        var result = Types.InAssembly(Core)
            .That().ResideInNamespace("Aduo.Core.Common")
            .ShouldNot().HaveDependencyOn("Aduo.Core.Modules")
            .GetResult();
        Assert.True(result.IsSuccessful, Describe(result));
    }

    private static string Describe(TestResult result)
        => "Vi phạm: " + string.Join(", ", result.FailingTypeNames ?? []);
}
