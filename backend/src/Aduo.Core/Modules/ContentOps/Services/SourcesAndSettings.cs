using System.Globalization;
using Aduo.Core.Common.Results;
using Aduo.Core.Common.Settings;
using Aduo.Core.Infrastructure.Persistence;
using Aduo.Core.Modules.ContentOps.Domain;
using Aduo.Core.Modules.ContentOps.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Aduo.Core.Modules.ContentOps.Services;

public sealed record LicenseResponse(short Id, string Code, string Name, string? Url, bool AllowsCommercial, bool RequiresAttribution, bool ShareAlike, bool AllowsModification);

public sealed record SourceResponse(
    Guid Id, string Name, string? Provider, string? Url, short DefaultLicenseId, string LicenseCode,
    bool LicenseAllowsCommercial, DateTimeOffset? LicenseVerifiedAt, string? Notes, bool IsActive,
    bool BlocksPublishing);

public sealed record CreateSourceRequest(string? Name, string? Provider, string? Url, short DefaultLicenseId, string? Notes);

/// <summary>Chỉ trường nào có giá trị (khác null) mới được cập nhật. <c>LicenseVerified</c> true = ghi ngày xác minh là hôm nay, false = bỏ xác minh.</summary>
public sealed record UpdateSourceRequest(
    string? Name, string? Provider, string? Url, short? DefaultLicenseId, string? Notes, bool? IsActive, bool? LicenseVerified);

public sealed record SettingResponse(string Key, decimal Value, decimal Default, decimal Min, decimal Max, string Description);

public sealed record MediaResponse(
    Guid Id, MediaKind Kind, string ContentType, string? FileName, long SizeBytes, string Checksum,
    Guid SourceId, short LicenseId, string? Attribution, string? OriginalUrl, DateTimeOffset CreatedAt);

/// <summary>Quản lý nguồn (US-50). Nguồn có giấy phép không cho thương mại bị đánh dấu chặn import và publish (BR-CNT-03).</summary>
public sealed class SourceService(AppDbContext db)
{
    private static readonly Error NameRequired = Error.Validation(ErrorCodes.ValidationFailed, "Vui lòng nhập tên nguồn.",
        new Dictionary<string, string[]> { ["name"] = ["Tên nguồn là bắt buộc (tối đa 120 ký tự)."] });

    private static readonly Error NameTaken = Error.Conflict("SOURCE_NAME_TAKEN", "Đã có nguồn với tên này.");

    public async Task<IReadOnlyList<LicenseResponse>> ListLicensesAsync(CancellationToken ct)
        => await db.Licenses.AsNoTracking().OrderBy(l => l.Id)
            .Select(l => new LicenseResponse(l.Id, l.Code, l.Name, l.Url, l.AllowsCommercial, l.RequiresAttribution, l.ShareAlike, l.AllowsModification))
            .ToListAsync(ct);

    public async Task<IReadOnlyList<SourceResponse>> ListAsync(CancellationToken ct)
    {
        var rows = await (from s in db.Sources.AsNoTracking()
                          join l in db.Licenses.AsNoTracking() on s.DefaultLicenseId equals l.Id
                          orderby s.Name
                          select new { s, l }).ToListAsync(ct);
        return rows.Select(r => ToResponse(r.s, r.l)).ToList();
    }

    public async Task<Result<SourceResponse>> CreateAsync(CreateSourceRequest req, CancellationToken ct)
    {
        var name = req.Name?.Trim();
        if (string.IsNullOrEmpty(name) || name.Length > 120)
        {
            return NameRequired;
        }

        var invalid = ValidateUrl(req.Url);
        if (invalid is not null)
        {
            return invalid;
        }

        var license = await db.Licenses.AsNoTracking().SingleOrDefaultAsync(l => l.Id == req.DefaultLicenseId, ct);
        if (license is null)
        {
            return ContentOpsErrors.LicenseNotFound;
        }

        if (await db.Sources.AnyAsync(s => s.Name == name, ct))
        {
            return NameTaken;
        }

        var source = new Source
        {
            Name = name,
            Provider = Clean(req.Provider, 120),
            Url = Clean(req.Url, 300),
            DefaultLicenseId = license.Id,
            Notes = Clean(req.Notes, 1000),
        };
        db.Sources.Add(source);
        await db.SaveChangesAsync(ct);
        return ToResponse(source, license);
    }

    public async Task<Result<SourceResponse>> UpdateAsync(Guid id, UpdateSourceRequest req, CancellationToken ct)
    {
        var source = await db.Sources.SingleOrDefaultAsync(s => s.Id == id, ct);
        if (source is null)
        {
            return Error.NotFound("Không tìm thấy nguồn.");
        }

        if (req.Name is not null)
        {
            var name = req.Name.Trim();
            if (name.Length is 0 or > 120)
            {
                return NameRequired;
            }

            if (name != source.Name && await db.Sources.AnyAsync(s => s.Name == name, ct))
            {
                return NameTaken;
            }

            source.Name = name;
        }

        var invalid = ValidateUrl(req.Url);
        if (invalid is not null)
        {
            return invalid;
        }

        if (req.DefaultLicenseId is { } licenseId)
        {
            if (!await db.Licenses.AnyAsync(l => l.Id == licenseId, ct))
            {
                return ContentOpsErrors.LicenseNotFound;
            }

            source.DefaultLicenseId = licenseId;
        }

        source.Provider = req.Provider is null ? source.Provider : Clean(req.Provider, 120);
        source.Url = req.Url is null ? source.Url : Clean(req.Url, 300);
        source.Notes = req.Notes is null ? source.Notes : Clean(req.Notes, 1000);
        source.IsActive = req.IsActive ?? source.IsActive;
        if (req.LicenseVerified is { } verified)
        {
            source.LicenseVerifiedAt = verified ? DateTimeOffset.UtcNow : null;
        }

        await db.SaveChangesAsync(ct);
        var license = await db.Licenses.AsNoTracking().SingleAsync(l => l.Id == source.DefaultLicenseId, ct);
        return ToResponse(source, license);
    }

    private static SourceResponse ToResponse(Source s, License l)
        => new(s.Id, s.Name, s.Provider, s.Url, s.DefaultLicenseId, l.Code, l.AllowsCommercial, s.LicenseVerifiedAt, s.Notes, s.IsActive,
            BlocksPublishing: !l.AllowsCommercial);

    private static string? Clean(string? s, int max)
    {
        var t = s?.Trim();
        return string.IsNullOrEmpty(t) ? null : t.Length <= max ? t : t[..max];
    }

    /// <summary>Chỉ nhận liên kết http/https (chặn javascript:, data: khi hiển thị trên trang ghi công).</summary>
    private static Error? ValidateUrl(string? url)
        => string.IsNullOrWhiteSpace(url) || (Uri.TryCreate(url.Trim(), UriKind.Absolute, out var u) && u.Scheme is "http" or "https")
            ? null
            : Error.Validation(ErrorCodes.ValidationFailed, "Liên kết không hợp lệ.",
                new Dictionary<string, string[]> { ["url"] = ["Chỉ chấp nhận liên kết http hoặc https."] });
}

/// <summary>Đọc và sửa cấu hình business rules (docs 02-contentops §4.6). Cache 1 phút, xóa ngay khi có thay đổi.</summary>
public sealed class SettingsService(AppDbContext db, IMemoryCache cache, TimeProvider time) : ISettings
{
    private static string CacheKey(string key) => "setting:" + key;

    public async Task<decimal> GetNumberAsync(string key, CancellationToken ct)
    {
        if (cache.TryGetValue(CacheKey(key), out decimal cached))
        {
            return cached;
        }

        var def = SettingDefinitions.All.FirstOrDefault(d => d.Key == key)
                  ?? throw new ArgumentException($"Cấu hình không tồn tại: {key}", nameof(key));
        var raw = await db.AppSettings.AsNoTracking().Where(s => s.Key == key).Select(s => s.Value).SingleOrDefaultAsync(ct);
        var value = raw is not null && decimal.TryParse(raw, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed) ? parsed : def.Default;

        cache.Set(CacheKey(key), value, TimeSpan.FromMinutes(1));
        return value;
    }

    public async Task<IReadOnlyList<SettingResponse>> ListAsync(CancellationToken ct)
    {
        var result = new List<SettingResponse>();
        foreach (var d in SettingDefinitions.All)
        {
            result.Add(new SettingResponse(d.Key, await GetNumberAsync(d.Key, ct), d.Default, d.Min, d.Max, d.Description));
        }

        return result;
    }

    public async Task<Result<IReadOnlyList<SettingResponse>>> UpdateAsync(IReadOnlyDictionary<string, decimal> changes, CancellationToken ct)
    {
        var errors = new Dictionary<string, string[]>();
        foreach (var (key, value) in changes)
        {
            var def = SettingDefinitions.All.FirstOrDefault(d => d.Key == key);
            if (def is null)
            {
                errors[key] = ["Cấu hình này không tồn tại."];
            }
            else if (value < def.Min || value > def.Max)
            {
                errors[key] = [$"Giá trị phải từ {def.Min} đến {def.Max}."];
            }
        }

        if (errors.Count > 0)
        {
            return Error.Validation(ErrorCodes.ValidationFailed, "Có giá trị cấu hình không hợp lệ.", errors);
        }

        var now = time.GetUtcNow();
        foreach (var (key, value) in changes)
        {
            var row = await db.AppSettings.SingleOrDefaultAsync(s => s.Key == key, ct);
            var json = value.ToString(CultureInfo.InvariantCulture);
            if (row is null)
            {
                db.AppSettings.Add(new AppSetting { Key = key, Value = json, UpdatedAt = now });
            }
            else
            {
                row.Value = json;
                row.UpdatedAt = now;
            }

            cache.Remove(CacheKey(key));
        }

        await db.SaveChangesAsync(ct);
        return Result<IReadOnlyList<SettingResponse>>.Ok(await ListAsync(ct));
    }
}
