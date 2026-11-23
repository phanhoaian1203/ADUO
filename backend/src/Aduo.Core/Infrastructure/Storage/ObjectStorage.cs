using Microsoft.Extensions.Options;

namespace Aduo.Core.Infrastructure.Storage;

/// <summary>
/// Kho lưu file (audio, hình, file import). Hiện có bản thư mục local cho dev; bản Cloudflare R2 (S3 API) thêm khi
/// chuẩn bị staging mà không phải sửa logic nghiệp vụ, vì mọi nơi chỉ dùng interface này.
/// </summary>
public interface IObjectStorage
{
    Task PutAsync(string key, Stream content, CancellationToken ct);

    /// <summary>Mở file để đọc. Trả null nếu không tồn tại.</summary>
    Task<Stream?> OpenReadAsync(string key, CancellationToken ct);

    Task DeleteAsync(string key, CancellationToken ct);
}

public sealed class StorageOptions
{
    public const string SectionName = "Storage";

    /// <summary>Thư mục gốc khi dùng kho local. Mặc định "storage" cạnh ứng dụng; đã nằm trong .gitignore.</summary>
    public string LocalPath { get; set; } = "storage";
}

/// <summary>
/// Lưu file vào thư mục local. Chặn path traversal: khóa chỉ gồm ký tự an toàn, và đường dẫn đã chuẩn hóa
/// phải nằm trong thư mục gốc. (Khóa do server tạo từ UUID, nhưng vẫn kiểm tra như một lớp bảo vệ thứ hai.)
/// </summary>
public sealed class LocalFileObjectStorage(IOptions<StorageOptions> options) : IObjectStorage
{
    private readonly string _root = Path.GetFullPath(options.Value.LocalPath);

    public async Task PutAsync(string key, Stream content, CancellationToken ct)
    {
        var path = ResolvePath(key);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await using var file = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, useAsync: true);
        await content.CopyToAsync(file, ct);
    }

    public Task<Stream?> OpenReadAsync(string key, CancellationToken ct)
    {
        var path = ResolvePath(key);
        Stream? stream = File.Exists(path)
            ? new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, useAsync: true)
            : null;
        return Task.FromResult(stream);
    }

    public Task DeleteAsync(string key, CancellationToken ct)
    {
        var path = ResolvePath(key);
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        return Task.CompletedTask;
    }

    private string ResolvePath(string key)
    {
        if (string.IsNullOrEmpty(key) || key.Any(c => !(char.IsAsciiLetterOrDigit(c) || c is '/' or '.' or '-' or '_'))
            || key.Contains("..", StringComparison.Ordinal) || key.StartsWith('/'))
        {
            throw new ArgumentException("Khóa lưu trữ không hợp lệ.", nameof(key));
        }

        var full = Path.GetFullPath(Path.Combine(_root, key));
        if (!full.StartsWith(_root + Path.DirectorySeparatorChar, StringComparison.Ordinal))
        {
            throw new ArgumentException("Khóa lưu trữ nằm ngoài thư mục gốc.", nameof(key));
        }

        return full;
    }
}
