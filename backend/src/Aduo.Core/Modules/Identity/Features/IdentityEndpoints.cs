using System.Security.Claims;
using Aduo.Core.Common.Http;
using Aduo.Core.Common.Results;
using Aduo.Core.Common.Security;
using Aduo.Core.Modules.Identity.Auth;
using Aduo.Core.Modules.Identity.Domain;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Aduo.Core.Modules.Identity.Features;

/// <summary>Tên policy rate limit của Identity. Cấu hình ở Aduo.Api/Setup/RateLimitingSetup.</summary>
public static class IdentityRateLimits
{
    public const string Login = "auth-login";
    public const string Register = "auth-register";
    public const string ForgotPassword = "auth-forgot";
}

public sealed record ExternalProvidersResponse(bool Google);

public static class IdentityEndpoints
{
    private const string GoogleScheme = "Google";

    public static IEndpointRouteBuilder MapIdentityEndpoints(this IEndpointRouteBuilder app)
    {
        MapAuth(app.MapGroup("/api/v1/auth").WithTags("Identity"));
        MapMe(app.MapGroup("/api/v1/me").WithTags("Identity").RequireAuthorization());
        MapAdmin(app.MapGroup("/api/v1/admin/users").WithTags("Identity Admin"));
        return app;
    }

    private static void MapAuth(RouteGroupBuilder g)
    {
        // Cấp cookie XSRF-TOKEN. Frontend gọi một lần khi tải trang.
        g.MapGet("/csrf", (HttpContext ctx, AuthCookies cookies) =>
        {
            cookies.EnsureCsrfCookie(ctx);
            return Results.NoContent();
        });

        g.MapPost("/register", async (RegisterRequest req, RegisterHandler h, AuthCookies cookies, HttpContext ctx, CancellationToken ct) =>
        {
            var result = await h.HandleAsync(req, Client(ctx), ct);
            if (!result.IsSuccess)
            {
                return result.Error.ToProblem();
            }

            cookies.Write(ctx.Response, result.Value.Tokens);
            return Results.Created("/api/v1/me", result.Value.Me);
        }).Produces<MeResponse>(StatusCodes.Status201Created).RequireRateLimiting(IdentityRateLimits.Register);

        g.MapPost("/login", async (LoginRequest req, LoginHandler h, AuthCookies cookies, HttpContext ctx, CancellationToken ct) =>
        {
            var result = await h.HandleAsync(req, Client(ctx), ct);
            if (!result.IsSuccess)
            {
                return result.Error.ToProblem();
            }

            cookies.Write(ctx.Response, result.Value.Tokens);
            return Results.Ok(result.Value.Me);
        }).Produces<MeResponse>().RequireRateLimiting(IdentityRateLimits.Login);

        // Làm mới token. Refresh token đọc từ cookie, KHÔNG nhận qua body/URL. Có kiểm tra CSRF (middleware).
        g.MapPost("/refresh", async (SessionService sessions, AuthCookies cookies, HttpContext ctx, CancellationToken ct) =>
        {
            var result = await sessions.RefreshAsync(cookies.ReadRefresh(ctx.Request), Client(ctx), ct);
            if (!result.IsSuccess)
            {
                // Refresh hỏng thì xóa cookie để client không lặp lại vô ích.
                cookies.Clear(ctx.Response);
                return result.Error.ToProblem();
            }

            cookies.Write(ctx.Response, result.Value);
            return Results.NoContent();
        }).RequireRateLimiting(IdentityRateLimits.Login);

        g.MapPost("/logout", async (SessionService sessions, AuthCookies cookies, HttpContext ctx, CancellationToken ct) =>
        {
            // Đăng xuất luôn thành công và xóa cookie, kể cả khi token đã hết hạn. Nếu còn đọc được phiên thì thu hồi.
            if (Guid.TryParse(ctx.User.FindFirstValue(AduoClaims.Subject), out var userId)
                && Guid.TryParse(ctx.User.FindFirstValue(AduoClaims.SessionId), out var sid))
            {
                await sessions.RevokeSessionAsync(userId, sid, RevokeReason.Logout, Client(ctx), ct);
            }

            cookies.Clear(ctx.Response);
            return Results.NoContent();
        }).AllowAnonymous();

        g.MapPost("/logout-all", async (SessionService sessions, AuthCookies cookies, HttpContext ctx, CancellationToken ct) =>
        {
            await sessions.RevokeAllAsync(UserId(ctx.User), RevokeReason.LogoutAll, Client(ctx), exceptSessionId: null, ct);
            cookies.Clear(ctx.Response);
            return Results.NoContent();
        }).RequireAuthorization().WithMetadata(new FreshSessionMetadata());

        g.MapPost("/confirm-email", async (ConfirmEmailRequest req, ConfirmEmailHandler h)
            => (await h.HandleAsync(req)).ToHttpResult());

        g.MapPost("/resend-confirmation", async (ResendConfirmationHandler h, HttpContext ctx, CancellationToken ct)
            => (await h.HandleAsync(UserId(ctx.User), Client(ctx), ct)).ToHttpResult()).RequireAuthorization();

        g.MapPost("/forgot-password", async (ForgotPasswordRequest req, ForgotPasswordHandler h, HttpContext ctx, CancellationToken ct) =>
        {
            await h.HandleAsync(req, Client(ctx), ct);
            return Results.Accepted(); // luôn 202, không cho biết email có tồn tại hay không
        }).RequireRateLimiting(IdentityRateLimits.ForgotPassword);

        g.MapPost("/reset-password", async (ResetPasswordRequest req, ResetPasswordHandler h, HttpContext ctx, CancellationToken ct)
            => (await h.HandleAsync(req, Client(ctx), ct)).ToHttpResult()).RequireRateLimiting(IdentityRateLimits.ForgotPassword);

        // ───────────── Đăng nhập Google (chỉ bật khi đã cấu hình ClientId/Secret) ─────────────
        g.MapGet("/external/providers", async (IAuthenticationSchemeProvider schemes)
            => Results.Ok(new ExternalProvidersResponse(await schemes.GetSchemeAsync(GoogleScheme) is not null))).Produces<ExternalProvidersResponse>();

        g.MapGet("/external/google", async (string? returnUrl, IAuthenticationSchemeProvider schemes) =>
        {
            if (await schemes.GetSchemeAsync(GoogleScheme) is null)
            {
                return Error.NotFound("Đăng nhập Google chưa được bật.").ToProblem();
            }

            var props = new AuthenticationProperties { RedirectUri = "/api/v1/auth/external/google/complete" };
            props.Items["returnUrl"] = IdentityRedirects.SafeReturnUrl(returnUrl);
            return Results.Challenge(props, [GoogleScheme]);
        });

        // Google gọi lại middleware (CallbackPath) → cookie tạm → tới đây để tạo phiên của ADUO.
        g.MapGet("/external/google/complete", async (
            ExternalLoginHandler h, AuthCookies cookies, HttpContext ctx, CancellationToken ct) =>
        {
            var external = await ctx.AuthenticateAsync(IdentityModule.ExternalScheme);
            await ctx.SignOutAsync(IdentityModule.ExternalScheme); // cookie tạm dùng một lần
            if (!external.Succeeded || external.Principal is null)
            {
                return Results.Redirect("/dang-nhap?loi=google_failed");
            }

            var p = external.Principal;
            var ext = new ExternalIdentity(
                "Google",
                p.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty,
                p.FindFirstValue(ClaimTypes.Email),
                string.Equals(p.FindFirstValue("email_verified"), "true", StringComparison.OrdinalIgnoreCase),
                p.FindFirstValue(ClaimTypes.Name));
            if (ext.ProviderKey.Length == 0)
            {
                return Results.Redirect("/dang-nhap?loi=google_failed");
            }

            var result = await h.HandleAsync(ext, Client(ctx), ct);
            var returnUrl = IdentityRedirects.SafeReturnUrl((external.Properties?.Items.TryGetValue("returnUrl", out var ru) == true ? ru : null));
            switch (result.Outcome)
            {
                case ExternalLoginOutcome.SignedIn:
                    cookies.Write(ctx.Response, result.Tokens!);
                    return Results.Redirect(returnUrl);
                case ExternalLoginOutcome.SignedUpNew:
                    cookies.Write(ctx.Response, result.Tokens!);
                    return Results.Redirect("/chon-muc-tieu?returnUrl=" + Uri.EscapeDataString(returnUrl));
                case ExternalLoginOutcome.EmailExistsNeedsPassword:
                    return Results.Redirect("/dang-nhap?loi=google_email_exists");
                case ExternalLoginOutcome.EmailNotVerified:
                    return Results.Redirect("/dang-nhap?loi=google_unverified");
                default:
                    return Results.Redirect("/dang-nhap?loi=google_unavailable");
            }
        });
    }

    private static void MapMe(RouteGroupBuilder g)
    {
        g.MapGet("", async (GetMeHandler h, HttpContext ctx) => (await h.HandleAsync(UserId(ctx.User))).ToHttpResult()).Produces<MeResponse>();

        g.MapPatch("", async (UpdateProfileRequest req, UpdateProfileHandler h, HttpContext ctx)
            => (await h.HandleAsync(UserId(ctx.User), req)).ToHttpResult()).Produces<MeResponse>();

        g.MapPost("/change-password", async (
            ChangePasswordRequest req, ChangePasswordHandler h, AuthCookies cookies, HttpContext ctx, CancellationToken ct) =>
        {
            var result = await h.HandleAsync(UserId(ctx.User), SessionId(ctx.User), req, Client(ctx), ct);
            if (!result.IsSuccess)
            {
                return result.Error.ToProblem();
            }

            if (result.Value is { } fresh)
            {
                cookies.Write(ctx.Response, fresh);
            }

            return Results.NoContent();
        }).WithMetadata(new FreshSessionMetadata());

        g.MapPost("/delete", async (DeleteAccountRequest req, DeleteAccountHandler h, AuthCookies cookies, HttpContext ctx, CancellationToken ct) =>
        {
            var result = await h.HandleAsync(UserId(ctx.User), req, Client(ctx), ct);
            if (result.IsSuccess)
            {
                cookies.Clear(ctx.Response);
            }

            return result.ToHttpResult();
        }).WithMetadata(new FreshSessionMetadata());

        // Thiết bị đang đăng nhập (US-70).
        g.MapGet("/sessions", async (SessionService sessions, HttpContext ctx, CancellationToken ct) =>
        {
            var list = await sessions.ListAsync(UserId(ctx.User), SessionId(ctx.User), ct);
            return Results.Ok(list.Select(s => new SessionResponse(s.SessionId, s.DeviceLabel, s.IpPrefix, s.CreatedAt, s.LastUsedAt, s.IsCurrent)));
        }).Produces<IEnumerable<SessionResponse>>();

        g.MapDelete("/sessions/{sessionId:guid}", async (Guid sessionId, SessionService sessions, HttpContext ctx, CancellationToken ct) =>
        {
            var ok = await sessions.RevokeSessionAsync(UserId(ctx.User), sessionId, RevokeReason.UserRevokedDevice, Client(ctx), ct);
            return ok ? Results.NoContent() : Error.NotFound().ToProblem();
        }).WithMetadata(new FreshSessionMetadata());
    }

    private static void MapAdmin(RouteGroupBuilder g)
    {
        // Admin: kiểm tra phiên trực tiếp trong DB (không đệm) + yêu cầu đăng nhập trong vòng 12 giờ (BR-AUTH-13, 15).
        g.RequireAuthorization(Permissions.UsersManage)
            .WithMetadata(new FreshSessionMetadata())
            .AddEndpointFilter(async (ctx, next) =>
            {
                var opt = ctx.HttpContext.RequestServices.GetRequiredService<IOptions<AuthOptions>>().Value;
                var time = ctx.HttpContext.RequestServices.GetRequiredService<TimeProvider>();
                var authTime = ctx.HttpContext.User.FindFirstValue(AduoClaims.AuthTime);
                if (!long.TryParse(authTime, out var seconds)
                    || time.GetUtcNow() - DateTimeOffset.FromUnixTimeSeconds(seconds) > TimeSpan.FromHours(opt.AdminReauthHours))
                {
                    return AuthErrors.ReauthRequired.ToProblem();
                }

                return await next(ctx);
            });

        g.MapGet("", async (AdminUserHandlers h, string? q, int? page, int? pageSize, CancellationToken ct)
            => Results.Ok(await h.ListAsync(q, page ?? 1, Math.Clamp(pageSize ?? 20, 1, 100), ct))).Produces<IReadOnlyList<AdminUserResponse>>();

        g.MapPost("/{id:guid}/lock", async (Guid id, AdminUserHandlers h, HttpContext ctx, CancellationToken ct)
            => (await h.LockAsync(id, Client(ctx), ct)).ToHttpResult());

        g.MapPost("/{id:guid}/unlock", async (Guid id, AdminUserHandlers h)
            => (await h.UnlockAsync(id)).ToHttpResult());
    }

    private static Guid UserId(ClaimsPrincipal user) => Guid.Parse(user.FindFirstValue(AduoClaims.Subject)!);

    private static Guid SessionId(ClaimsPrincipal user) => Guid.Parse(user.FindFirstValue(AduoClaims.SessionId)!);

    private static ClientInfo Client(HttpContext ctx)
        => ClientInfo.From(ctx.Connection.RemoteIpAddress, ctx.Request.Headers.UserAgent.ToString());
}
