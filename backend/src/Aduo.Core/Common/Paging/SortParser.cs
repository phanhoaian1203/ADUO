using Aduo.Core.Common.Results;

namespace Aduo.Core.Common.Paging;

/// <summary>Một tiêu chí sắp xếp đã được kiểm tra.</summary>
public sealed record SortField(string Field, bool Descending);

/// <summary>
/// Phân tích chuỗi sort kiểu "-createdAt,title" (dấu "-" = giảm dần).
/// Chỉ chấp nhận các trường nằm trong whitelist của từng endpoint, để client không thể
/// sắp xếp theo cột tùy ý (lộ dữ liệu hoặc gây truy vấn chậm).
/// </summary>
public static class SortParser
{
    public static Result<IReadOnlyList<SortField>> Parse(string? sort, IReadOnlyCollection<string> allowedFields)
    {
        if (string.IsNullOrWhiteSpace(sort))
        {
            return Result<IReadOnlyList<SortField>>.Ok([]);
        }

        var fields = new List<SortField>();
        foreach (var raw in sort.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var descending = raw.StartsWith('-');
            var name = descending ? raw[1..] : raw;

            var match = allowedFields.FirstOrDefault(a => string.Equals(a, name, StringComparison.OrdinalIgnoreCase));
            if (match is null)
            {
                return Error.Validation(ErrorCodes.InvalidSort, $"Không thể sắp xếp theo '{name}'.");
            }

            if (fields.Any(f => f.Field == match))
            {
                continue; // bỏ qua trường lặp lại
            }

            fields.Add(new SortField(match, descending));
        }

        return fields;
    }
}
