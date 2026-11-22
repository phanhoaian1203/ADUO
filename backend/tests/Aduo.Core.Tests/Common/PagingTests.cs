using Aduo.Core.Common.Paging;
using Aduo.Core.Common.Results;

namespace Aduo.Core.Tests.Common;

public class PagingTests
{
    private static readonly string[] Allowed = ["createdAt", "title"];

    [Theory]
    [InlineData(null, null, 1, 20)]
    [InlineData(0, 0, 1, 1)]
    [InlineData(-5, 500, 1, 100)]
    [InlineData(3, 10, 3, 10)]
    public void PageRequest_ClampsValues(int? page, int? size, int expectedPage, int expectedSize)
    {
        var req = new PageRequest(page, size);
        Assert.Equal(expectedPage, req.Page);
        Assert.Equal(expectedSize, req.PageSize);
    }

    [Fact]
    public void PageRequest_Skip_IsComputedFromPage()
        => Assert.Equal(40, new PageRequest(3, 20).Skip);

    [Fact]
    public void SortParser_ParsesDescendingAndAscending()
    {
        var result = SortParser.Parse("-createdAt, title", Allowed);

        Assert.True(result.IsSuccess);
        Assert.Equal([new SortField("createdAt", true), new SortField("title", false)], result.Value);
    }

    [Fact]
    public void SortParser_IsCaseInsensitive_AndReturnsCanonicalName()
    {
        var result = SortParser.Parse("CREATEDAT", Allowed);
        Assert.Equal("createdAt", Assert.Single(result.Value!).Field);
    }

    [Fact]
    public void SortParser_RejectsFieldOutsideWhitelist() // F-T07
    {
        var result = SortParser.Parse("-hacked", Allowed);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.InvalidSort, result.Error!.Code);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void SortParser_EmptyInput_ReturnsNoFields(string? sort)
        => Assert.Empty(SortParser.Parse(sort, Allowed).Value!);

    [Fact]
    public void SortParser_IgnoresDuplicateFields()
        => Assert.Single(SortParser.Parse("title,-title", Allowed).Value!);
}
