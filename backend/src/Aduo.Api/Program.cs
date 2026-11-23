using System.Text.Json.Serialization;
using Aduo.Api.Middleware;
using Aduo.Api.Setup;
using Aduo.Core;
using Aduo.Core.Modules.Identity;
using Aduo.Core.Modules.Identity.Auth;
using Aduo.Core.Modules.Identity.Features;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Serilog;
using Serilog.Formatting.Compact;

// Đọc kỹ docs/phase-4/modules/00-foundation.md §3–§4 để hiểu luồng một request đi qua file này.
var builder = WebApplication.CreateBuilder(args);

// ── Logging: Serilog. Mức log đọc từ appsettings "Serilog", sink cấu hình bằng code ───
// Dev: log dạng chữ dễ đọc. Môi trường khác: JSON có cấu trúc ra console (Docker thu lại)
// và file xoay vòng 14 ngày. Không bao giờ log mật khẩu, token, cookie.
builder.Host.UseSerilog((context, services, config) =>
{
    config.ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext();

    if (context.HostingEnvironment.IsDevelopment())
    {
        config.WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {TraceId}{NewLine}{Exception}");
    }
    else
    {
        var formatter = new RenderedCompactJsonFormatter();
        config.WriteTo.Console(formatter);
        if (!context.HostingEnvironment.IsEnvironment("Testing"))
        {
            config.WriteTo.File(formatter, context.Configuration["Logging:FilePath"] ?? "logs/aduo-.log",
                rollingInterval: RollingInterval.Day, retainedFileCountLimit: 14);
        }
    }
});

// ── Services ───────────────────────────────────────────────────────────────────────
builder.Services.AddAduoCore(builder.Configuration);
builder.Services.AddIdentityModule(builder.Configuration);

// ── Đăng nhập Google: chỉ bật khi đã cấu hình ClientId/ClientSecret (user-secrets ở dev, biến môi trường ở production) ──
var googleSection = builder.Configuration.GetSection("Authentication:Google");
if (!string.IsNullOrEmpty(googleSection["ClientId"]) && !string.IsNullOrEmpty(googleSection["ClientSecret"]))
{
    var secureCookies = builder.Configuration.GetValue("Auth:UseSecureCookies", true);
    builder.Services.AddAuthentication().AddGoogle(o =>
    {
        o.ClientId = googleSection["ClientId"]!;
        o.ClientSecret = googleSection["ClientSecret"]!;
        o.SignInScheme = IdentityModule.ExternalScheme;
        o.CallbackPath = "/api/v1/auth/external/google/callback"; // phải khớp "Authorized redirect URI" trên Google Cloud
        o.SaveTokens = false; // không lưu token của Google (ID-T45)
        o.ClaimActions.MapJsonKey("email_verified", "email_verified", ClaimValueTypes.Boolean);
        // Cookie "correlation" mặc định SameSite=None (cần HTTPS). Callback của Google là GET điều hướng cấp cao nên Lax là đủ.
        o.CorrelationCookie.SameSite = SameSiteMode.Lax;
        o.CorrelationCookie.SecurePolicy = secureCookies ? CookieSecurePolicy.Always : CookieSecurePolicy.SameAsRequest;
        o.Events.OnRemoteFailure = ctx =>
        {
            // Người dùng bấm Hủy ở Google, hoặc state không hợp lệ: quay về trang đăng nhập, không lộ chi tiết.
            ctx.Response.Redirect("/dang-nhap?loi=google_failed");
            ctx.HandleResponse();
            return Task.CompletedTask;
        };
    });
}
builder.Services.AddAduoBackgroundJobs();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

builder.Services.AddAduoProblemDetails();
builder.Services.AddAduoRateLimiting(builder.Configuration);
builder.Services.AddAduoHealthChecks();
builder.Services.AddOpenApi("v1");

// Ở production, Caddy đứng trước API: tin header X-Forwarded-* để biết IP thật và scheme https.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
    options.KnownIPNetworks.Clear(); // Caddy nằm trong mạng Docker nội bộ, IP không cố định
    options.KnownProxies.Clear();
});

var app = builder.Build();

// ── Middleware pipeline (thứ tự quan trọng, xem 00-foundation.md §4.4) ─────────────
app.UseForwardedHeaders();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseSerilogRequestLogging();
app.UseRateLimiter();
// Thứ tự: xác thực (đọc JWT từ cookie) → CSRF → phân quyền.
app.UseAuthentication();
app.UseMiddleware<CsrfMiddleware>();
app.UseAuthorization();

// ── Endpoints ──────────────────────────────────────────────────────────────────────
app.MapAduoHealthChecks();
app.MapIdentityEndpoints();

if (!app.Environment.IsProduction())
{
    // OpenAPI chỉ bật ở Development/Staging. Frontend sinh kiểu TypeScript từ file này.
    app.MapOpenApi("/api/openapi/{documentName}.json");
}

if (app.Environment.IsEnvironment("Testing"))
{
    // Chỉ dùng cho integration test F-T03: kiểm tra lỗi 500 không lộ chi tiết.
    app.MapGet("/api/_test/throw", () => { throw new InvalidOperationException("secret internal detail"); });
}

app.Run();

/// <summary>Cho phép WebApplicationFactory trong integration test truy cập Program.</summary>
public partial class Program;
