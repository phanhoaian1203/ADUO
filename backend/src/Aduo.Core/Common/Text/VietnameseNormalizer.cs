using System.Globalization;
using System.Text;

namespace Aduo.Core.Common.Text;

/// <summary>
/// Chuẩn hóa tiếng Việt để tìm kiếm không dấu: "Cô gái" → "co gai", "Đi làm" → "di lam".
/// Cách làm: tách ký tự thành chữ gốc + dấu (Unicode NFD), bỏ các dấu, rồi xử lý riêng đ/Đ
/// (đ không phải là "d + dấu" trong Unicode nên NFD không tách được).
/// </summary>
public static class VietnameseNormalizer
{
    public static string ForSearch(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }

        var decomposed = input.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(decomposed.Length);
        var lastWasSpace = false;

        foreach (var ch in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

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
            sb.Append(ch == 'đ' ? 'd' : ch);
        }

        return sb.ToString().Normalize(NormalizationForm.FormC);
    }
}
