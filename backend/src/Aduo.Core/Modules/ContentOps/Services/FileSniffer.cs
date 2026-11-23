using System.Text;
using Aduo.Core.Modules.ContentOps.Domain;

namespace Aduo.Core.Modules.ContentOps.Services;

public sealed record SniffedFile(MediaKind Kind, string ContentType, string Extension, long MaxBytes);

/// <summary>
/// Nhận diện loại file bằng MAGIC BYTES (nội dung thật), không tin phần mở rộng hay Content-Type do client gửi
/// (docs 02-contentops §4.2). File ".mp3" thực chất là ".exe" sẽ không khớp loại nào và bị từ chối.
/// </summary>
public static class FileSniffer
{
    public const long MaxAudioBytes = 5 * 1024 * 1024;
    public const long MaxImageBytes = 2 * 1024 * 1024;
    public const long MaxImportBytes = 50 * 1024 * 1024;

    /// <param name="header">Các byte đầu của file (nên ≥ 8 KB để kiểm tra văn bản).</param>
    /// <param name="fileName">Chỉ dùng để phân biệt .csv và .json cho file văn bản, không dùng để quyết định audio/hình.</param>
    public static SniffedFile? Detect(ReadOnlySpan<byte> header, string? fileName)
    {
        if (header.Length >= 3 && header[0] == 'I' && header[1] == 'D' && header[2] == '3')
        {
            return new(MediaKind.Audio, "audio/mpeg", "mp3", MaxAudioBytes);
        }

        // MP3 không có thẻ ID3: khung đồng bộ 11 bit 1 (0xFFE) kèm phiên bản/lớp hợp lệ.
        if (header.Length >= 2 && header[0] == 0xFF && (header[1] & 0xE0) == 0xE0 && (header[1] & 0x18) != 0x08 && (header[1] & 0x06) != 0x00)
        {
            return new(MediaKind.Audio, "audio/mpeg", "mp3", MaxAudioBytes);
        }

        if (header.Length >= 4 && header[..4].SequenceEqual("OggS"u8))
        {
            return new(MediaKind.Audio, "audio/ogg", "ogg", MaxAudioBytes);
        }

        if (header.Length >= 8 && header[..8].SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }))
        {
            return new(MediaKind.Image, "image/png", "png", MaxImageBytes);
        }

        if (header.Length >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF)
        {
            return new(MediaKind.Image, "image/jpeg", "jpg", MaxImageBytes);
        }

        if (header.Length >= 12 && header[..4].SequenceEqual("RIFF"u8) && header.Slice(8, 4).SequenceEqual("WEBP"u8))
        {
            return new(MediaKind.Image, "image/webp", "webp", MaxImageBytes);
        }

        return DetectText(header, fileName);
    }

    private static SniffedFile? DetectText(ReadOnlySpan<byte> header, string? fileName)
    {
        var ext = Path.GetExtension(fileName ?? string.Empty).ToLowerInvariant();
        if (ext is not (".csv" or ".json") || header.IndexOf((byte)0) >= 0)
        {
            return null; // có byte NUL thì gần như chắc chắn là file nhị phân
        }

        string text;
        try
        {
            // Throw khi không phải UTF-8 hợp lệ. Cắt ở ranh giới an toàn: bỏ tối đa 3 byte cuối có thể là ký tự dang dở.
            var strict = new UTF8Encoding(false, throwOnInvalidBytes: true);
            var slice = header.Length > 8192 ? header[..8192] : header;
            text = TryDecode(strict, slice);
        }
        catch (DecoderFallbackException)
        {
            return null;
        }

        if (ext == ".json")
        {
            var first = text.TrimStart('﻿', ' ', '\t', '\r', '\n').FirstOrDefault();
            return first is '{' or '[' ? new(MediaKind.ImportFile, "application/json", "json", MaxImportBytes) : null;
        }

        return new(MediaKind.ImportFile, "text/csv", "csv", MaxImportBytes);
    }

    private static string TryDecode(UTF8Encoding strict, ReadOnlySpan<byte> bytes)
    {
        for (var cut = 0; cut <= 3 && cut < bytes.Length; cut++)
        {
            try
            {
                return strict.GetString(bytes[..(bytes.Length - cut)]);
            }
            catch (DecoderFallbackException) when (cut < 3)
            {
                // thử bỏ bớt byte cuối (ký tự đa byte bị cắt dở)
            }
        }

        return strict.GetString(bytes);
    }
}
