using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Aduo.Api.IntegrationTests;

/// <summary>
/// Client giả lập trình duyệt: tự giữ cookie (có xét Path), tự gắn header X-XSRF-TOKEN.
/// Không dùng cookie container sẵn của HttpClient để test đọc được thuộc tính Set-Cookie (HttpOnly, SameSite, Path).
/// </summary>
public sealed class TestClient
{
    private readonly HttpClient _http;
    private readonly Dictionary<string, (string Value, string Path)> _jar = new();

    public TestClient(ApiFactory factory)
        => _http = factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = false, AllowAutoRedirect = false });

    /// <summary>Mọi Set-Cookie của response gần nhất (nguyên văn) để test kiểm tra thuộc tính.</summary>
    public List<string> LastSetCookies { get; private set; } = [];

    public string? Cookie(string name) => _jar.TryGetValue(name, out var c) ? c.Value : null;

    public void SetCookie(string name, string value, string path = "/") => _jar[name] = (value, path);

    public void RemoveCookie(string name) => _jar.Remove(name);

    public async Task EnsureCsrfAsync()
    {
        if (Cookie("XSRF-TOKEN") is null)
        {
            await SendAsync(HttpMethod.Get, "/api/v1/auth/csrf");
        }
    }

    public async Task<HttpResponseMessage> SendAsync(
        HttpMethod method,
        string url,
        object? body = null,
        bool csrf = true,
        string? bearer = null,
        bool sendCookies = true)
    {
        var req = new HttpRequestMessage(method, url);
        if (body is not null)
        {
            req.Content = JsonContent.Create(body);
        }

        if (sendCookies)
        {
            var path = new Uri(_http.BaseAddress!, url).AbsolutePath;
            var cookies = _jar.Where(kv => path.StartsWith(kv.Value.Path, StringComparison.Ordinal))
                .Select(kv => $"{kv.Key}={kv.Value.Value}");
            var header = string.Join("; ", cookies);
            if (header.Length > 0)
            {
                req.Headers.Add("Cookie", header);
            }
        }

        if (csrf && method != HttpMethod.Get && Cookie("XSRF-TOKEN") is { } xsrf)
        {
            req.Headers.Add("X-XSRF-TOKEN", xsrf);
        }

        if (bearer is not null)
        {
            req.Headers.Add("Authorization", "Bearer " + bearer);
        }

        req.Headers.Add("User-Agent", "Mozilla/5.0 (Linux; Android 13) Chrome/120 Mobile Safari/537.36");

        var res = await _http.SendAsync(req);
        Absorb(res);
        return res;
    }

    public Task<HttpResponseMessage> PostAsync(string url, object? body = null, bool csrf = true)
        => SendAsync(HttpMethod.Post, url, body ?? new { }, csrf);

    public Task<HttpResponseMessage> GetAsync(string url) => SendAsync(HttpMethod.Get, url);

    private void Absorb(HttpResponseMessage res)
    {
        LastSetCookies = res.Headers.TryGetValues("Set-Cookie", out var values) ? values.ToList() : [];
        foreach (var raw in LastSetCookies)
        {
            var parts = raw.Split(';', StringSplitOptions.TrimEntries);
            var eq = parts[0].IndexOf('=');
            var name = parts[0][..eq];
            var value = parts[0][(eq + 1)..];
            var path = parts.Skip(1).FirstOrDefault(p => p.StartsWith("path=", StringComparison.OrdinalIgnoreCase))?[5..] ?? "/";
            var expires = parts.Skip(1).FirstOrDefault(p => p.StartsWith("expires=", StringComparison.OrdinalIgnoreCase))?[8..];
            var expired = expires is not null && DateTimeOffset.TryParse(expires, out var e) && e < DateTimeOffset.UtcNow.AddYears(-1);

            if (value.Length == 0 || expired)
            {
                _jar.Remove(name);
            }
            else
            {
                _jar[name] = (value, path);
            }
        }
    }

    public static async Task<JsonElement> JsonAsync(HttpResponseMessage res)
        => JsonDocument.Parse(await res.Content.ReadAsStringAsync()).RootElement.Clone();

    /// <summary>Mã lỗi ổn định "code" trong ProblemDetails.</summary>
    public static async Task<string?> CodeAsync(HttpResponseMessage res)
    {
        var json = await JsonAsync(res);
        return json.TryGetProperty("code", out var c) ? c.GetString() : null;
    }
}

public static class TestUsers
{
    public const string Password = "Matkhau12345";

    private static int _n;

    public static string NewEmail() => $"user{Interlocked.Increment(ref _n)}-{Guid.NewGuid():N}"[..24] + "@example.com";

    public static object Register(string email, string? password = null) => new
    {
        displayName = "Lan",
        email,
        password = password ?? Password,
        acceptTerms = true,
        termsVersion = "2026-11",
        privacyVersion = "2026-11",
    };

    /// <summary>Đăng ký + đăng nhập sẵn, trả client đã có cookie.</summary>
    public static async Task<(TestClient Client, string Email)> SignedInAsync(ApiFactory factory)
    {
        var client = new TestClient(factory);
        await client.EnsureCsrfAsync();
        var email = NewEmail();
        var res = await client.PostAsync("/api/v1/auth/register", Register(email));
        Assert.Equal(HttpStatusCode.Created, res.StatusCode);
        return (client, email);
    }
}
