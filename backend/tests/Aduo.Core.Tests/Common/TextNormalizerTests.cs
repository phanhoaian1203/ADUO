using Aduo.Core.Common.Text;

namespace Aduo.Core.Tests.Common;

public class TextNormalizerTests
{
    [Theory]
    [InlineData("Straße", "strasse")] // F-T05
    [InlineData("STRASSE", "strasse")]
    [InlineData("Mädchen", "maedchen")]
    [InlineData("  Grüße   aus  Köln ", "gruesse aus koeln")]
    [InlineData("ẞ", "ss")]
    [InlineData("", "")]
    [InlineData(null, "")]
    public void German_ForSearch_NormalizesUmlautsCaseAndSpaces(string? input, string expected)
        => Assert.Equal(expected, GermanTextNormalizer.ForSearch(input));

    [Theory]
    [InlineData("Đi làm", "di lam")] // F-T06
    [InlineData("Cô gái", "co gai")]
    [InlineData("  Gọi   điện ", "goi dien")]
    [InlineData("MẸ", "me")]
    [InlineData("", "")]
    [InlineData(null, "")]
    public void Vietnamese_ForSearch_RemovesDiacritics(string? input, string expected)
        => Assert.Equal(expected, VietnameseNormalizer.ForSearch(input));
}
