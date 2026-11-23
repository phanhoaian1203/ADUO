using System.Security.Claims;
using Aduo.Core.Common.Http;
using Aduo.Core.Common.Privacy;
using Aduo.Core.Common.Security;
using Aduo.Core.Infrastructure.Email;
using Aduo.Core.Infrastructure.Jobs;
using Aduo.Core.Infrastructure.Persistence;
using Aduo.Core.Modules.Identity.Auth;
using Aduo.Core.Modules.Identity.Domain;
using Aduo.Core.Modules.Identity.Features;
using Aduo.Core.Modules.Identity.Jobs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Aduo.Core.Modules.Identity;

/// <summary>Đăng ký toàn bộ dịch vụ của module Identity (docs/phase-4/modules/01-identity.md).</summary>
public static class IdentityModule
{
    public const string ExternalScheme = "Aduo.External";

    public static IServiceCollection AddIdentityModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<AuthOptions>(configuration.GetSection(AuthOptions.SectionName));
        services.Configure<EmailOptions>(configuration.GetSection(EmailOptions.SectionName));

        // Khóa JWT sai/thiếu thì dừng ngay khi khởi động, không chạy với cấu hình yếu.
        var jwt = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
        var signingKeys = jwt.BuildKeys();

        // Khóa Data Protection lưu trong DB để deploy không làm hết hiệu lực link email đã gửi.
        services.AddDataProtection().SetApplicationName("aduo").PersistKeysToDbContext<AppDbContext>();

        services.AddIdentityUserStore();

        services.AddScoped<AccessTokenService>();
        services.AddScoped<AuthEventLogger>();
        services.AddScoped<SessionService>();
        services.AddScoped<SessionValidator>();
        services.AddSingleton<EmailTokenService>();
        services.AddSingleton<AuthCookies>();
        services.AddMemoryCache();

        services.AddScoped<RegisterHandler>();
        services.AddScoped<LoginHandler>();
        services.AddScoped<ExternalLoginHandler>();
        services.AddScoped<ConfirmEmailHandler>();
        services.AddScoped<ResendConfirmationHandler>();
        services.AddScoped<ForgotPasswordHandler>();
        services.AddScoped<ResetPasswordHandler>();
        services.AddScoped<ChangePasswordHandler>();
        services.AddScoped<UpdateProfileHandler>();
        services.AddScoped<GetMeHandler>();
        services.AddScoped<DeleteAccountHandler>();
        services.AddScoped<AdminUserHandlers>();

        services.AddScoped<IUserDataEraser, IdentityDataEraser>();
        services.AddRecurringJob<PurgeDeletedAccountsJob>(TimeSpan.FromHours(6));
        services.AddRecurringJob<CleanupExpiredSessionsJob>(TimeSpan.FromHours(24));

        if (configuration.GetSection(EmailOptions.SectionName).GetValue<string>("SmtpHost") is { Length: > 0 })
        {
            services.AddSingleton<IEmailSender, SmtpEmailSender>();
        }
        else
        {
            services.AddSingleton<IEmailSender, LoggingEmailSender>();
        }

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(o =>
            {
                o.MapInboundClaims = false; // giữ nguyên tên claim "sub", "sid"...
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKeys = signingKeys,
                    ValidAlgorithms = [SecurityAlgorithms.HmacSha256], // chặn alg=none và đổi thuật toán
                    ValidateLifetime = true,
                    RequireExpirationTime = true,
                    ClockSkew = TimeSpan.FromSeconds(5),
                    NameClaimType = AduoClaims.Subject,
                    RoleClaimType = ClaimTypes.Role,
                };
                o.Events = new JwtBearerEvents
                {
                    OnMessageReceived = ctx =>
                    {
                        // Web: token nằm trong cookie HttpOnly. Native/test: header Authorization (mặc định của JwtBearer).
                        if (string.IsNullOrEmpty(ctx.Request.Headers.Authorization))
                        {
                            ctx.Token = ctx.HttpContext.RequestServices.GetRequiredService<AuthCookies>().ReadAccess(ctx.Request);
                        }

                        return Task.CompletedTask;
                    },
                    OnTokenValidated = async ctx =>
                    {
                        var principal = ctx.Principal!;
                        if (!Guid.TryParse(principal.FindFirstValue(AduoClaims.Subject), out var userId)
                            || !Guid.TryParse(principal.FindFirstValue(AduoClaims.SessionId), out var sessionId))
                        {
                            ctx.Fail("malformed");
                            return;
                        }

                        // Endpoint Admin/nhạy cảm bỏ qua bộ đệm để thu hồi có hiệu lực tức thì (BR-AUTH-15).
                        var bypass = ctx.HttpContext.GetEndpoint()?.Metadata.GetMetadata<FreshSessionMetadata>() is not null;
                        var validator = ctx.HttpContext.RequestServices.GetRequiredService<SessionValidator>();
                        var status = await validator.GetStatusAsync(sessionId, userId, bypass, ctx.HttpContext.RequestAborted);

                        if (status is null || !status.Active || status.IsLocked)
                        {
                            ctx.HttpContext.Items["auth_error"] = AuthErrors.TokenRevoked;
                            ctx.Fail("revoked");
                            return;
                        }

                        if (!string.Equals(status.StampFingerprint, principal.FindFirstValue(AduoClaims.StampFingerprint), StringComparison.Ordinal))
                        {
                            ctx.HttpContext.Items["auth_error"] = AuthErrors.TokenRevoked;
                            ctx.Fail("stamp");
                            return;
                        }

                        ((ClaimsIdentity)principal.Identity!).AddClaims(SessionValidator.BuildClaims(status));
                    },
                    OnAuthenticationFailed = ctx =>
                    {
                        if (ctx.Exception is SecurityTokenExpiredException)
                        {
                            ctx.HttpContext.Items["auth_error"] = AuthErrors.TokenExpired;
                        }

                        return Task.CompletedTask;
                    },
                    OnChallenge = async ctx =>
                    {
                        ctx.HandleResponse();
                        var error = ctx.HttpContext.Items["auth_error"] as Common.Results.Error
                                    ?? new Common.Results.Error(Common.Results.ErrorCodes.Unauthenticated, "Bạn chưa đăng nhập.", Common.Results.ErrorType.Unauthorized);
                        await error.ToProblem().ExecuteAsync(ctx.HttpContext);
                    },
                };
            })
            // Cookie tạm (10 phút) giữ kết quả đăng nhập Google giữa lúc Google gọi lại và lúc ta tạo phiên. KHÔNG phải cookie đăng nhập.
            .AddCookie(ExternalScheme, o =>
            {
                o.Cookie.Name = "aduo_ext";
                o.Cookie.HttpOnly = true;
                o.Cookie.SameSite = SameSiteMode.Lax;
                o.Cookie.SecurePolicy = configuration.GetValue("Auth:UseSecureCookies", true) ? CookieSecurePolicy.Always : CookieSecurePolicy.SameAsRequest;
                o.ExpireTimeSpan = TimeSpan.FromMinutes(10);
            });

        // Dùng chung TimeProvider với phần còn lại của hệ thống để kiểm thử hết hạn bằng đồng hồ giả (ID-T25).
        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<TimeProvider>((o, time) => o.TokenValidationParameters.LifetimeValidator = (notBefore, expires, _, p) =>
            {
                var now = time.GetUtcNow().UtcDateTime;
                if (expires is null || expires.Value + p.ClockSkew < now)
                {
                    throw new SecurityTokenExpiredException("Token đã hết hạn.") { Expires = expires ?? default };
                }

                if (notBefore is not null && notBefore.Value - p.ClockSkew > now)
                {
                    throw new SecurityTokenNotYetValidException("Token chưa có hiệu lực.");
                }

                return true;
            });

        services.AddAuthorization(o =>
        {
            // Mỗi permission một policy cùng tên. Endpoint yêu cầu permission, không yêu cầu tên vai trò.
            foreach (var permission in Permissions.All)
            {
                o.AddPolicy(permission, p => p.RequireAuthenticatedUser().RequireClaim(AduoClaims.Permission, permission));
            }
        });

        return services;
    }
}

/// <summary>Đánh dấu endpoint cần kiểm tra phiên trực tiếp trong DB (bỏ qua bộ đệm 60 giây).</summary>
public sealed class FreshSessionMetadata;

/// <summary>BR-AUTH-02: không trùng email, không nằm trong danh sách mật khẩu phổ biến.</summary>
public sealed class AduoPasswordValidator : IPasswordValidator<AppUser>
{
    private static readonly HashSet<string> Common = new(StringComparer.OrdinalIgnoreCase)
    {
        "12345678", "123456789", "1234567890", "password", "password1", "matkhau123", "matkhau1", "qwerty123",
        "11111111", "00000000", "abc12345", "abcd1234", "iloveyou1", "admin123", "12341234", "123123123",
    };

    public Task<IdentityResult> ValidateAsync(UserManager<AppUser> manager, AppUser user, string? password)
    {
        var errors = new List<IdentityError>();
        if (password is not null)
        {
            var hasLetter = password.Any(char.IsLetter);
            var hasDigit = password.Any(char.IsDigit);
            if (password.Length < 8 || !hasLetter || !hasDigit)
            {
                errors.Add(new IdentityError { Code = "PasswordWeak", Description = "Mật khẩu cần ít nhất 8 ký tự, gồm cả chữ và số." });
            }

            if (Common.Contains(password))
            {
                errors.Add(new IdentityError { Code = "PasswordCommon", Description = "Mật khẩu này quá phổ biến, hãy chọn mật khẩu khác." });
            }

            if (!string.IsNullOrEmpty(user.Email) && password.Contains(user.Email, StringComparison.OrdinalIgnoreCase))
            {
                errors.Add(new IdentityError { Code = "PasswordEmail", Description = "Mật khẩu không được chứa email." });
            }
        }

        return Task.FromResult(errors.Count == 0 ? IdentityResult.Success : IdentityResult.Failed([.. errors]));
    }
}

public static class IdentityUserStoreExtensions
{
    /// <summary>
    /// Phần quản lý người dùng của ASP.NET Core Identity (UserManager, RoleManager, quy tắc mật khẩu, khóa tạm).
    /// Identity CHỈ quản lý người dùng, KHÔNG dùng cookie đăng nhập của nó (AD-09). Dùng chung cho API và CLI.
    /// </summary>
    public static IServiceCollection AddIdentityUserStore(this IServiceCollection services)
    {
        services.AddIdentityCore<AppUser>(o =>
            {
                o.User.RequireUniqueEmail = true;
                o.Password.RequiredLength = 1; // quy tắc thật nằm ở AduoPasswordValidator (thông báo tiếng Việt)
                o.Password.RequireDigit = false;
                o.Password.RequireLowercase = false;
                o.Password.RequireUppercase = false;
                o.Password.RequireNonAlphanumeric = false;
                o.Lockout.AllowedForNewUsers = true;
                o.Lockout.MaxFailedAccessAttempts = 5;
                o.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddRoles<AppRole>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddPasswordValidator<AduoPasswordValidator>();

        return services;
    }
}
