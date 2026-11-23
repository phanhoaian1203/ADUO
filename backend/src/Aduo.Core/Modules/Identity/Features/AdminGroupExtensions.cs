using System.Security.Claims;
using Aduo.Core.Common.Http;
using Aduo.Core.Modules.Identity.Auth;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Aduo.Core.Modules.Identity.Features;

/// <summary>
/// Cổng bảo vệ dùng chung cho mọi nhóm endpoint Admin (Identity, ContentOps…). Gom một chỗ để mọi trang quản trị
/// đều có cùng ba lớp bảo vệ: (1) quyền cụ thể, (2) kiểm tra phiên trực tiếp trong DB không qua bộ đệm,
/// (3) yêu cầu đăng nhập lại nếu quá AdminReauthHours (BR-AUTH-13, BR-AUTH-15).
/// </summary>
public static class AdminGroupExtensions
{
    public static RouteGroupBuilder RequireAdmin(this RouteGroupBuilder group, string permission)
    {
        group.RequireAuthorization(permission)
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
        return group;
    }

    /// <summary>Id người đang đăng nhập (claim sub). Chỉ gọi trong endpoint đã yêu cầu xác thực.</summary>
    public static Guid CurrentUserId(this HttpContext ctx) => Guid.Parse(ctx.User.FindFirstValue(AduoClaims.Subject)!);
}
