using System.Security.Cryptography;
using Aduo.Core.Common.Results;
using Aduo.Core.Infrastructure.Persistence;
using Aduo.Core.Infrastructure.Storage;
using Aduo.Core.Modules.ContentOps.Domain;
using Microsoft.EntityFrameworkCore;

namespace Aduo.Core.Modules.ContentOps.Services;

public static class ContentOpsErrors
{
    public static readonly Error InvalidFileType = Error.Validation("INVALID_FILE_TYPE", "Loại file không được hỗ trợ hoặc nội dung không khớp (cho phép mp3, ogg, png, jpg, webp, csv, json).");
    public static readonly Error SourceNotFound = Error.Validation("SOURCE_NOT_FOUND", "Không tìm thấy nguồn.");
    public static readonly Error LicenseNotFound = Error.Validation("LICENSE_NOT_FOUND", "Không tìm thấy giấy phép.");
    public static readonly Error SourceInactive = Error.Rule("SOURCE_INACTIVE", "Nguồn này đang bị tắt.");

    public static Error FileTooLarge(long maxBytes)
        => Error.Validation("FILE_TOO_LARGE", $"File quá lớn (tối đa {maxBytes / (1024 * 1024)} MB cho loại này).");

    /// <summary>BR-CNT-03 (PD-05): nguồn/giấy phép không cho dùng thương mại thì không import, không upload, không publish.</summary>
    public static Error LicenseNotCommercial(string licenseCode)
        => Error.Rule("LICENSE_NOT_COMMERCIAL", $"Giấy phép {licenseCode} không cho phép dùng thương mại nên nguồn này không import hay publish được.");
}

public sealed record UploadMediaRequest(
    string? FileName,
    Stream Content,
    Guid SourceId,
    short? LicenseId,
    string? Attribution,
    string? OriginalUrl,
    Guid? UploadedBy,
    MediaKind? ExpectedKind = null);

public sealed record UploadMediaResult(MediaAsset Media, bool Reused);

/// <summary>
/// Upload file an toàn (docs 02-contentops §4.2): nhận diện bằng magic bytes, giới hạn kích thước theo loại,
/// tên lưu là UUID, trùng checksum thì trả lại file cũ, ghi kho trước rồi mới ghi DB và dọn file nếu DB lỗi.
/// </summary>
public sealed class MediaService(AppDbContext db, IObjectStorage storage, TimeProvider time)
{
    private const int HeaderBytes = 8192;

    public async Task<Result<UploadMediaResult>> UploadAsync(UploadMediaRequest req, CancellationToken ct)
    {
        var source = await db.Sources.AsNoTracking().SingleOrDefaultAsync(s => s.Id == req.SourceId, ct);
        if (source is null)
        {
            return ContentOpsErrors.SourceNotFound;
        }

        if (!source.IsActive)
        {
            return ContentOpsErrors.SourceInactive;
        }

        // Audio từ nguồn ngoài phải có license riêng cho từng file (BR-CNT-11): mặc định lấy license của nguồn, ghi đè được.
        var licenseId = req.LicenseId ?? source.DefaultLicenseId;
        var license = await db.Licenses.AsNoTracking().SingleOrDefaultAsync(l => l.Id == licenseId, ct);
        if (license is null)
        {
            return ContentOpsErrors.LicenseNotFound;
        }

        if (!license.AllowsCommercial)
        {
            return ContentOpsErrors.LicenseNotCommercial(license.Code);
        }

        // 1) Đọc phần đầu để nhận diện loại bằng nội dung thật.
        var header = new byte[HeaderBytes];
        var read = await ReadFullAsync(req.Content, header, ct);
        var sniffed = FileSniffer.Detect(header.AsSpan(0, read), req.FileName);
        if (sniffed is null || (req.ExpectedKind is { } expected && sniffed.Kind != expected))
        {
            return ContentOpsErrors.InvalidFileType;
        }

        // 2) Đọc nốt phần còn lại nhưng không vượt giới hạn của loại này (+1 byte để biết là vượt).
        var buffer = new MemoryStream();
        buffer.Write(header, 0, read);
        var remaining = sniffed.MaxBytes + 1 - read;
        if (remaining > 0)
        {
            await CopyLimitedAsync(req.Content, buffer, remaining, ct);
        }

        if (buffer.Length > sniffed.MaxBytes)
        {
            return ContentOpsErrors.FileTooLarge(sniffed.MaxBytes);
        }

        // 3) Trùng checksum thì dùng lại, không lưu file mới.
        buffer.Position = 0;
        var checksum = Convert.ToHexStringLower(await SHA256.HashDataAsync(buffer, ct));
        var existing = await db.MediaAssets.AsNoTracking()
            .FirstOrDefaultAsync(m => m.ChecksumSha256 == checksum && m.Kind == sniffed.Kind, ct);
        if (existing is not null)
        {
            return new UploadMediaResult(existing, Reused: true);
        }

        // 4) Ghi kho trước, ghi DB sau. R2 và PostgreSQL không chung transaction nên DB lỗi thì xóa file vừa ghi (bù trừ).
        var now = time.GetUtcNow();
        var asset = new MediaAsset
        {
            Kind = sniffed.Kind,
            StorageKey = $"{sniffed.Kind.ToString().ToLowerInvariant()}/{now.Year}/{Guid.CreateVersion7():N}.{sniffed.Extension}",
            ContentType = sniffed.ContentType,
            OriginalFileName = SanitizeName(req.FileName),
            SizeBytes = buffer.Length,
            ChecksumSha256 = checksum,
            OriginalUrl = Truncate(req.OriginalUrl, 500),
            Attribution = Truncate(req.Attribution, 500),
            UploadedBy = req.UploadedBy,
            Meta = new Common.Content.ContentMeta
            {
                SourceId = source.Id,
                LicenseId = license.Id,
                Origin = Common.Content.ContentOrigin.HumanAuthored,
            },
        };

        buffer.Position = 0;
        await storage.PutAsync(asset.StorageKey, buffer, ct);
        try
        {
            db.MediaAssets.Add(asset);
            await db.SaveChangesAsync(ct);
        }
        catch
        {
            await storage.DeleteAsync(asset.StorageKey, CancellationToken.None);
            throw;
        }

        return new UploadMediaResult(asset, Reused: false);
    }

    private static async Task<int> ReadFullAsync(Stream s, byte[] buffer, CancellationToken ct)
    {
        var total = 0;
        while (total < buffer.Length)
        {
            var n = await s.ReadAsync(buffer.AsMemory(total), ct);
            if (n == 0)
            {
                break;
            }

            total += n;
        }

        return total;
    }

    private static async Task CopyLimitedAsync(Stream from, Stream to, long limit, CancellationToken ct)
    {
        var chunk = new byte[81920];
        long copied = 0;
        while (copied < limit)
        {
            var n = await from.ReadAsync(chunk.AsMemory(0, (int)Math.Min(chunk.Length, limit - copied)), ct);
            if (n == 0)
            {
                break;
            }

            await to.WriteAsync(chunk.AsMemory(0, n), ct);
            copied += n;
        }
    }

    /// <summary>Tên hiển thị đã làm sạch: bỏ đường dẫn và ký tự điều khiển. KHÔNG dùng để tạo đường dẫn lưu.</summary>
    private static string? SanitizeName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        var leaf = name.Replace('\\', '/').Split('/').Last();
        var clean = new string(leaf.Where(c => !char.IsControl(c)).ToArray()).Trim();
        return clean.Length == 0 ? null : Truncate(clean, 200);
    }

    private static string? Truncate(string? s, int max) => s is null ? null : s.Length <= max ? s : s[..max];
}
