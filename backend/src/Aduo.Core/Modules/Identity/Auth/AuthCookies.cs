using System.Security.Cryptography;
using Aduo.Core.Common.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Aduo.Core.Modules.Identity.Auth;

/// <summary>Đọc/ghi cookie xác thực. Thuộc tính cookie: docs/phase-4/modules/01-identity.md §14.7.</summary>
public sealed class AuthCookies(IOptions<AuthOptions> options)
{
    public const string CsrfCookie = "XSRF-TOKEN";
    public const string CsrfHeader = "X-XSRF-TOKEN";

    private readonly AuthOptions _opt = options.Value;

    public void Write(HttpResponse response, SessionTokens t)
    {
        // Access: HttpOnly, hết hạn cùng token (10 phút). Gửi kèm mọi request tới API.
        response.Cookies.Append(_opt.AccessCookieName, t.AccessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = _opt.UseSecureCookies,
            SameSite = SameSiteMode.Lax,
            Path = _opt.AccessCookiePath,
            Expires = t.AccessExpiresAt,
            IsEssential = true,
        });

        // Refresh: HttpOnly, chỉ gửi tới /api/v1/auth. Không đặt Expires nếu không "Ghi nhớ" (cookie phiên trình duyệt).
        if (t.RefreshToken is not null) response.Cookies.Append(_opt.RefreshCookieName, t.RefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = _opt.UseSecureCookies,
            SameSite = SameSiteMode.Lax,
            Path = _opt.RefreshCookiePath,
            Expires = t.PersistentCookie ? t.RefreshExpiresAt : null,
            IsEssential = true,
        });
    }

    public void Clear(HttpResponse response)
    {
        response.Cookies.Delete(_opt.AccessCookieName, new CookieOptions
        {
            Path = _opt.AccessCookiePath, Secure = _opt.UseSecureCookies, HttpOnly = true, SameSite = SameSiteMode.Lax,
        });
        response.Cookies.Delete(_opt.RefreshCookieName, new CookieOptions
        {
            Path = _opt.RefreshCookiePath, Secure = _opt.UseSecureCookies, HttpOnly = true, SameSite = SameSiteMode.Lax,
        });
    }

    public string? ReadAccess(HttpRequest request) => request.Cookies[_opt.AccessCookieName];

    public string? ReadRefresh(HttpRequest request) => request.Cookies[_opt.RefreshCookieName];

    public bool HasAuthCookies(HttpRequest request)
        => request.Cookies.ContainsKey(_opt.AccessCookieName) || request.Cookies.ContainsKey(_opt.RefreshCookieName);

    /// <summary>Cấp (hoặc giữ nguyên) cookie XSRF-TOKEN cho JavaScript đọc và gửi lại qua header.</summary>
    public void EnsureCsrfCookie(HttpContext ctx)
    {
        if (ctx.Request.Cookies.ContainsKey(CsrfCookie))
        {
            return;
        }

        var value = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        ctx.Response.Cookies.Append(CsrfCookie, value, new CookieOptions
        {
            HttpOnly = false, // JS phải đọc được để gắn vào header
            Secure = _opt.UseSecureCookies,
            SameSite = SameSiteMode.Lax,
            Path = "/",
            IsEssential = true,
        });
    }
}

/// <summary>
/// Chống CSRF bằng double-submit cookie: mọi request ghi (POST/PUT/PATCH/DELETE) phải có header X-XSRF-TOKEN
/// trùng cookie XSRF-TOKEN. Ngoại lệ: request chỉ dùng Authorization: Bearer, không kèm cookie xác thực
/// (trình duyệt không tự gắn Bearer nên không có nguy cơ CSRF).
/// </summary>
public sealed class CsrfMiddleware(RequestDelegate next, AuthCookies cookies, IOptions<AuthOptions> options)
{
    public async Task Invoke(HttpContext ctx)
    {
        if (RequiresCheck(ctx.Request) && !IsValid(ctx.Request))
        {
            await AuthErrors.CsrfInvalid.ToProblem().ExecuteAsync(ctx);
            return;
        }

        await next(ctx);
    }

    private bool RequiresCheck(HttpRequest r)
    {
        if (HttpMethods.IsGet(r.Method) || HttpMethods.IsHead(r.Method) || HttpMethods.IsOptions(r.Method) || HttpMethods.IsTrace(r.Method))
        {
            return false;
        }

        if (!r.Path.StartsWithSegments("/api"))
        {
            return false;
        }

        var bearerOnly = r.Headers.Authorization.ToString().StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                         && !cookies.HasAuthCookies(r);
        return !bearerOnly;
    }

    private bool IsValid(HttpRequest r)
    {
        var cookie = r.Cookies[AuthCookies.CsrfCookie];
        var header = r.Headers[AuthCookies.CsrfHeader].ToString();
        if (string.IsNullOrEmpty(cookie) || string.IsNullOrEmpty(header))
        {
            return false;
        }

        var equal = CryptographicOperations.FixedTimeEquals(
            System.Text.Encoding.UTF8.GetBytes(cookie), System.Text.Encoding.UTF8.GetBytes(header));
        if (!equal)
        {
            return false;
        }

        // Lớp bảo vệ thêm (tùy chọn): nếu có cấu hình danh sách Origin thì Origin phải nằm trong đó.
        var allowed = options.Value.AllowedOrigins;
        if (allowed.Count > 0 && r.Headers.Origin.ToString() is { Length: > 0 } origin)
        {
            return allowed.Contains(origin, StringComparer.OrdinalIgnoreCase);
        }

        return true;
    }
}
