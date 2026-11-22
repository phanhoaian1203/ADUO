namespace Aduo.Api.Middleware;

/// <summary>
/// Header bảo mật cho mọi response của API (docs/phase-3/01-system-architecture.md §8).
/// CSP của trang web do Next.js đặt riêng. API chỉ trả JSON nên CSP ở đây rất chặt.
/// HSTS chỉ bật ngoài môi trường Development (dev chạy http://localhost).
/// </summary>
public sealed class SecurityHeadersMiddleware(RequestDelegate next, IWebHostEnvironment env)
{
    public Task InvokeAsync(HttpContext context)
    {
        context.Response.OnStarting(() =>
        {
            var headers = context.Response.Headers;
            headers["X-Content-Type-Options"] = "nosniff";
            headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
            headers["X-Frame-Options"] = "DENY";
            headers["Content-Security-Policy"] = "default-src 'none'; frame-ancestors 'none'";
            headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
            if (!env.IsDevelopment())
            {
                headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
            }

            return Task.CompletedTask;
        });

        return next(context);
    }
}
