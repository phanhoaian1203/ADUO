using System.Text;

namespace Aduo.Core.Common.Text;

/// <summary>
/// Chuẩn hóa chữ tiếng Đức để tìm kiếm (BR-LEX-04):
/// chữ thường, ä→ae, ö→oe, ü→ue, ß/ẞ→ss, gộp khoảng trắng.
/// Nhờ vậy "Mädchen", "MAEDCHEN", "maedchen" đều so khớp được với nhau.
/// </summary>
public static class GermanTextNormalizer
{
    public static string ForSearch(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }

        var lower = input.Trim().ToLowerInvariant();
        var sb = new StringBuilder(lower.Length + 4);
        var lastWasSpace = false;

        foreach (var ch in lower)
        {
            if (char.IsWhiteSpace(ch))
            {
                if (!lastWasSpace)
                {
                    sb.Append(' ');
                }

                lastWasSpace = true;
                continue;
            }

            lastWasSpace = false;
            switch (ch)
            {
                case 'ä': sb.Append("ae"); break;
                case 'ö': sb.Append("oe"); break;
                case 'ü': sb.Append("ue"); break;
                case 'ß': sb.Append("ss"); break;
                default: sb.Append(ch); break;
            }
        }

        return sb.ToString();
    }
}
