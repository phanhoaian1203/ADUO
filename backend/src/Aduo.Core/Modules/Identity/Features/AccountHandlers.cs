using System.Text.Json;
using Aduo.Core.Common.Results;
using Aduo.Core.Common.Security;
using Aduo.Core.Infrastructure.Email;
using Aduo.Core.Infrastructure.Persistence;
using Aduo.Core.Modules.Identity.Auth;
using Aduo.Core.Modules.Identity.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Aduo.Core.Modules.Identity.Features;

/// <summary>Kết quả đăng nhập/đăng ký: hồ sơ trả cho client + token để endpoint ghi vào cookie.</summary>
public sealed record AuthResult(MeResponse Me, SessionTokens Tokens);

/// <summary>Đăng ký (UC-01). BR-AUTH-01..04, BR-PRIV-01.</summary>
public sealed class RegisterHandler(
    UserManager<AppUser> users,
    AppDbContext db,
    SessionService sessions,
    AuthEventLogger events,
    EmailTokenService tokens,
    IEmailSender email,
    TimeProvider time,
    IOptions<AuthOptions> options,
    ILogger<RegisterHandler> logger)
{
    public async Task<Result<AuthResult>> HandleAsync(RegisterRequest req, ClientInfo client, CancellationToken ct)
    {
        var fieldErrors = IdentityValidation.Register(req);
        if (fieldErrors.Count > 0)
        {
            return Error.Validation(ErrorCodes.ValidationFailed, "Vui lòng kiểm tra lại thông tin.", fieldErrors);
        }

        var opt = options.Value;
        var address = IdentityValidation.NormalizeEmail(req.Email);

        if (await users.FindByEmailAsync(address) is not null)
        {
            return IdentityErrors.EmailTaken;
        }

        var user = new AppUser
        {
            UserName = address,
            Email = address,
            DisplayName = req.DisplayName!.Trim(),
            CreatedAt = time.GetUtcNow(),
        };

        var created = await users.CreateAsync(user, req.Password!);
        if (!created.Succeeded)
        {
            if (created.Errors.Any(e => e.Code is "DuplicateEmail" or "DuplicateUserName"))
            {
                return IdentityErrors.EmailTaken; // race giữa hai request cùng email
            }

            return Error.Validation(ErrorCodes.ValidationFailed, "Vui lòng kiểm tra lại thông tin.",
                new Dictionary<string, string[]> { ["password"] = created.Errors.Select(e => e.Description).ToArray() });
        }

        await users.AddToRoleAsync(user, Roles.Student);

        db.UserConsents.Add(new UserConsent
        {
            UserId = user.Id,
            TermsVersion = req.TermsVersion ?? opt.TermsVersion,
            PrivacyVersion = req.PrivacyVersion ?? opt.PrivacyVersion,
            MinorGuardianConfirmed = req.MinorGuardianConfirmed,
            ConsentedAt = time.GetUtcNow(),
        });
        await db.SaveChangesAsync(ct);

        await ConfirmationEmail.SendAsync(user, tokens, email, events, db, opt, client, logger, ct);

        var session = await sessions.StartAsync(user, isAdmin: false, rememberMe: false, ClientKind.Web, client, ct);
        events.Add(AuthEventKind.LoginSucceeded, user.Id, client);
        await db.SaveChangesAsync(ct);

        return new AuthResult(IdentityValidation.ToMe(user, [Roles.Student], hasPassword: true), session);
    }
}

/// <summary>Gửi email xác minh (dùng chung cho đăng ký và gửi lại).</summary>
internal static class ConfirmationEmail
{
    public static async Task SendAsync(
        AppUser user, EmailTokenService tokens, IEmailSender email, AuthEventLogger events, AppDbContext db,
        AuthOptions opt, ClientInfo client, ILogger logger, CancellationToken ct)
    {
        var token = tokens.Create(EmailTokenService.ConfirmEmail, user.Id, Binding(user), TimeSpan.FromHours(24));
        var link = $"{opt.AppBaseUrl.TrimEnd('/')}/xac-minh-email?userId={user.Id}&token={token}";

        try
        {
            await email.SendAsync(new EmailMessage(
                user.Email!,
                "Xác minh email ADUO",
                $"Chào {user.DisplayName},\n\nBấm vào liên kết sau để xác minh email (hiệu lực 24 giờ):\n{link}\n\nNếu bạn không đăng ký ADUO, hãy bỏ qua email này."), ct);
            events.Add(AuthEventKind.ConfirmationEmailSent, user.Id, client);
            await db.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            // Gửi email lỗi không được làm hỏng đăng ký. Người dùng có thể bấm "Gửi lại".
            logger.LogWarning(ex, "Không gửi được email xác minh");
        }
    }

    public static string Binding(AppUser user) => TokenHashing.StampFingerprint(user.NormalizedEmail);
}

/// <summary>Đăng nhập (UC-02). BR-AUTH-06: sai 5 lần khóa 15 phút, lỗi chung chung.</summary>
public sealed class LoginHandler(
    UserManager<AppUser> users,
    AppDbContext db,
    SessionService sessions,
    AuthEventLogger events,
    TimeProvider time)
{
    public async Task<Result<AuthResult>> HandleAsync(LoginRequest req, ClientInfo client, CancellationToken ct)
    {
        var address = IdentityValidation.NormalizeEmail(req.Email);
        var user = string.IsNullOrEmpty(address) ? null : await users.FindByEmailAsync(address);

        if (user is null || user.IsDeleted)
        {
            // Băm giả để thời gian phản hồi không tiết lộ email có tồn tại hay không.
            users.PasswordHasher.HashPassword(new AppUser(), req.Password ?? string.Empty);
            events.Add(AuthEventKind.LoginFailed, null, client);
            await db.SaveChangesAsync(ct);
            return IdentityErrors.InvalidCredentials;
        }

        if (await users.IsLockedOutAsync(user))
        {
            var remaining = (user.LockoutEnd ?? time.GetUtcNow()) - time.GetUtcNow();
            return IdentityErrors.Locked(remaining < TimeSpan.Zero ? TimeSpan.Zero : remaining);
        }

        if (string.IsNullOrEmpty(req.Password) || !await users.CheckPasswordAsync(user, req.Password))
        {
            await users.AccessFailedAsync(user);
            var lockedNow = await users.IsLockedOutAsync(user);
            events.Add(lockedNow ? AuthEventKind.LockedOut : AuthEventKind.LoginFailed, user.Id, client);
            await db.SaveChangesAsync(ct);
            return lockedNow ? IdentityErrors.Locked(TimeSpan.FromMinutes(15)) : IdentityErrors.InvalidCredentials;
        }

        await users.ResetAccessFailedCountAsync(user);
        var roles = await users.GetRolesAsync(user);
        var isAdmin = roles.Contains(Roles.Admin);
        var session = await sessions.StartAsync(user, isAdmin, req.RememberMe, ClientKind.Web, client, ct);
        events.Add(AuthEventKind.LoginSucceeded, user.Id, client);
        await db.SaveChangesAsync(ct);

        return new AuthResult(IdentityValidation.ToMe(user, roles, user.PasswordHash is not null), session);
    }
}

public sealed class ConfirmEmailHandler(UserManager<AppUser> users, EmailTokenService tokens)
{
    public async Task<Result> HandleAsync(ConfirmEmailRequest req)
    {
        var user = await users.FindByIdAsync(req.UserId.ToString());
        if (user is null || user.IsDeleted)
        {
            return IdentityErrors.TokenInvalid;
        }

        if (user.EmailConfirmed)
        {
            return Result.Ok(); // bấm lại link: không báo lỗi
        }

        switch (tokens.Validate(EmailTokenService.ConfirmEmail, user.Id, ConfirmationEmail.Binding(user), req.Token))
        {
            case TokenCheck.Expired:
                return IdentityErrors.TokenExpired;
            case TokenCheck.Invalid:
                return IdentityErrors.TokenInvalid;
        }

        user.EmailConfirmed = true;
        var updated = await users.UpdateAsync(user);
        return updated.Succeeded ? Result.Ok() : IdentityErrors.TokenInvalid;
    }
}

/// <summary>Gửi lại email xác minh, tối đa 3 lần/giờ/người dùng (đếm qua auth_events).</summary>
public sealed class ResendConfirmationHandler(
    UserManager<AppUser> users,
    AppDbContext db,
    EmailTokenService tokens,
    IEmailSender email,
    AuthEventLogger events,
    TimeProvider time,
    IOptions<AuthOptions> options,
    ILogger<ResendConfirmationHandler> logger)
{
    public async Task<Result> HandleAsync(Guid userId, ClientInfo client, CancellationToken ct)
    {
        var user = await users.FindByIdAsync(userId.ToString());
        if (user is null || user.IsDeleted)
        {
            return Error.NotFound();
        }

        if (user.EmailConfirmed)
        {
            return Result.Ok();
        }

        var since = time.GetUtcNow().AddHours(-1);
        var sent = await db.AuthEvents.CountAsync(
            e => e.UserId == userId && e.Kind == AuthEventKind.ConfirmationEmailSent && e.OccurredAt > since, ct);
        if (sent >= 3)
        {
            return new Error(ErrorCodes.RateLimited, "Bạn đã yêu cầu gửi lại quá nhiều lần. Vui lòng thử lại sau.", ErrorType.RateLimited);
        }

        await ConfirmationEmail.SendAsync(user, tokens, email, events, db, options.Value, client, logger, ct);
        return Result.Ok();
    }
}

/// <summary>Quên mật khẩu. Luôn trả thành công để không lộ email nào đã đăng ký (US-04).</summary>
public sealed class ForgotPasswordHandler(
    UserManager<AppUser> users,
    AppDbContext db,
    EmailTokenService tokens,
    IEmailSender email,
    AuthEventLogger events,
    TimeProvider time,
    IOptions<AuthOptions> options,
    ILogger<ForgotPasswordHandler> logger)
{
    public async Task HandleAsync(ForgotPasswordRequest req, ClientInfo client, CancellationToken ct)
    {
        var address = IdentityValidation.NormalizeEmail(req.Email);
        var user = string.IsNullOrEmpty(address) ? null : await users.FindByEmailAsync(address);
        if (user is null || user.IsDeleted)
        {
            return;
        }

        var since = time.GetUtcNow().AddHours(-1);
        var recent = await db.AuthEvents.CountAsync(
            e => e.UserId == user.Id && e.Kind == AuthEventKind.PasswordResetRequested && e.OccurredAt > since, ct);
        events.Add(AuthEventKind.PasswordResetRequested, user.Id, client);
        await db.SaveChangesAsync(ct);
        if (recent >= 3)
        {
            return; // vẫn trả 202 nhưng không gửi thêm
        }

        var token = tokens.Create(EmailTokenService.ResetPassword, user.Id, TokenHashing.StampFingerprint(user.SecurityStamp), TimeSpan.FromHours(1));
        var link = $"{options.Value.AppBaseUrl.TrimEnd('/')}/dat-lai-mat-khau?email={Uri.EscapeDataString(user.Email!)}&token={token}";
        try
        {
            await email.SendAsync(new EmailMessage(
                user.Email!,
                "Đặt lại mật khẩu ADUO",
                $"Chào {user.DisplayName},\n\nBấm vào liên kết sau để đặt lại mật khẩu (hiệu lực 1 giờ):\n{link}\n\nNếu bạn không yêu cầu, hãy bỏ qua email này. Mật khẩu của bạn không thay đổi."), ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Không gửi được email đặt lại mật khẩu");
        }
    }
}

internal static class PasswordChanges
{
    /// <summary>Chạy validator mật khẩu (BR-AUTH-02) rồi đặt băm mới và đổi SecurityStamp (vô hiệu hóa token/phiên cũ).</summary>
    public static async Task<Result> SetAsync(UserManager<AppUser> users, AppUser user, string newPassword)
    {
        var errors = new List<string>();
        foreach (var validator in users.PasswordValidators)
        {
            var r = await validator.ValidateAsync(users, user, newPassword);
            if (!r.Succeeded)
            {
                errors.AddRange(r.Errors.Select(e => e.Description));
            }
        }

        if (errors.Count > 0)
        {
            return Error.Validation(ErrorCodes.ValidationFailed, "Mật khẩu chưa đạt yêu cầu.",
                new Dictionary<string, string[]> { ["newPassword"] = errors.ToArray() });
        }

        user.PasswordHash = users.PasswordHasher.HashPassword(user, newPassword);
        var stamp = await users.UpdateSecurityStampAsync(user); // lưu cả PasswordHash
        return stamp.Succeeded ? Result.Ok() : Error.Validation(ErrorCodes.ValidationFailed, "Không đổi được mật khẩu.");
    }
}

/// <summary>Đặt lại mật khẩu bằng link email. Thành công thì thu hồi mọi phiên (BR-AUTH-08).</summary>
public sealed class ResetPasswordHandler(
    UserManager<AppUser> users,
    EmailTokenService tokens,
    SessionService sessions,
    AuthEventLogger events,
    AppDbContext db)
{
    public async Task<Result> HandleAsync(ResetPasswordRequest req, ClientInfo client, CancellationToken ct)
    {
        var user = await users.FindByEmailAsync(IdentityValidation.NormalizeEmail(req.Email));
        if (user is null || user.IsDeleted || string.IsNullOrEmpty(req.NewPassword))
        {
            return IdentityErrors.TokenInvalid;
        }

        switch (tokens.Validate(EmailTokenService.ResetPassword, user.Id, TokenHashing.StampFingerprint(user.SecurityStamp), req.Token))
        {
            case TokenCheck.Expired:
                return IdentityErrors.TokenExpired;
            case TokenCheck.Invalid:
                return IdentityErrors.TokenInvalid;
        }

        var set = await PasswordChanges.SetAsync(users, user, req.NewPassword);
        if (!set.IsSuccess)
        {
            return set;
        }

        // Người đã đọc email thì chứng minh được quyền sở hữu email.
        if (!user.EmailConfirmed)
        {
            user.EmailConfirmed = true;
            await users.UpdateAsync(user);
        }

        await users.ResetAccessFailedCountAsync(user);
        await sessions.RevokeAllAsync(user.Id, RevokeReason.PasswordChanged, client, exceptSessionId: null, ct);
        events.Add(AuthEventKind.PasswordResetCompleted, user.Id, client);
        await db.SaveChangesAsync(ct);
        return Result.Ok();
    }
}

/// <summary>Đổi mật khẩu khi đã đăng nhập. Thu hồi các phiên KHÁC, giữ phiên hiện tại.</summary>
public sealed class ChangePasswordHandler(UserManager<AppUser> users, SessionService sessions, AuthEventLogger events, AppDbContext db)
{
    public async Task<Result<SessionTokens?>> HandleAsync(Guid userId, Guid sessionId, ChangePasswordRequest req, ClientInfo client, CancellationToken ct)
    {
        var user = await users.FindByIdAsync(userId.ToString());
        if (user is null || user.IsDeleted || string.IsNullOrEmpty(req.NewPassword))
        {
            return Error.NotFound();
        }

        // Tài khoản chỉ có Google (chưa có mật khẩu) thì được đặt mật khẩu lần đầu mà không cần mật khẩu cũ.
        if (user.PasswordHash is not null && (string.IsNullOrEmpty(req.CurrentPassword) || !await users.CheckPasswordAsync(user, req.CurrentPassword)))
        {
            return IdentityErrors.WrongPassword;
        }

        var set = await PasswordChanges.SetAsync(users, user, req.NewPassword);
        if (!set.IsSuccess)
        {
            return set.Error;
        }

        await sessions.RevokeAllAsync(user.Id, RevokeReason.PasswordChanged, client, exceptSessionId: sessionId, ct);

        // SecurityStamp đã đổi nên access token hiện tại sẽ bị từ chối. Cấp lại cặp token cho phiên này để người dùng không bị đá ra.
        await sessions.RevokeSessionAsync(user.Id, sessionId, RevokeReason.PasswordChanged, client, ct);
        var roles = await users.GetRolesAsync(user);
        var fresh = await sessions.StartAsync(user, roles.Contains(Roles.Admin), rememberMe: false, ClientKind.Web, client, ct);

        events.Add(AuthEventKind.PasswordChanged, user.Id, client);
        await db.SaveChangesAsync(ct);
        return Result<SessionTokens?>.Ok(fresh);
    }
}

public sealed class UpdateProfileHandler(UserManager<AppUser> users)
{
    public async Task<Result<MeResponse>> HandleAsync(Guid userId, UpdateProfileRequest req)
    {
        var user = await users.FindByIdAsync(userId.ToString());
        if (user is null || user.IsDeleted)
        {
            return Error.NotFound();
        }

        if (req.DisplayName is not null)
        {
            var error = IdentityValidation.DisplayName(req.DisplayName);
            if (error is not null)
            {
                return Error.Validation(ErrorCodes.ValidationFailed, "Vui lòng kiểm tra lại thông tin.",
                    new Dictionary<string, string[]> { ["displayName"] = [error] });
            }

            user.DisplayName = req.DisplayName.Trim();
        }

        // Email KHÔNG nằm trong UpdateProfileRequest nên không thể đổi qua đây (ID-T15).
        user.LearningPurpose = req.LearningPurpose ?? user.LearningPurpose;
        user.TargetLevel = req.TargetLevel ?? user.TargetLevel;
        user.AvatarMediaId = req.AvatarMediaId ?? user.AvatarMediaId;

        await users.UpdateAsync(user);
        var roles = await users.GetRolesAsync(user);
        return IdentityValidation.ToMe(user, roles, user.PasswordHash is not null);
    }
}

public sealed class GetMeHandler(UserManager<AppUser> users)
{
    public async Task<Result<MeResponse>> HandleAsync(Guid userId)
    {
        var user = await users.FindByIdAsync(userId.ToString());
        if (user is null || user.IsDeleted)
        {
            return Error.NotFound();
        }

        var roles = await users.GetRolesAsync(user);
        return IdentityValidation.ToMe(user, roles, user.PasswordHash is not null);
    }
}

/// <summary>Xóa tài khoản (BR-PRIV-02): đánh dấu xóa + thu hồi mọi phiên; job ẩn danh sau vài ngày.</summary>
public sealed class DeleteAccountHandler(
    UserManager<AppUser> users,
    SessionService sessions,
    AuthEventLogger events,
    AppDbContext db,
    TimeProvider time)
{
    public async Task<Result> HandleAsync(Guid userId, DeleteAccountRequest req, ClientInfo client, CancellationToken ct)
    {
        var user = await users.FindByIdAsync(userId.ToString());
        if (user is null || user.IsDeleted)
        {
            return Error.NotFound();
        }

        if (!string.Equals(req.ConfirmText?.Trim(), "XÓA", StringComparison.OrdinalIgnoreCase))
        {
            return Error.Validation(ErrorCodes.ValidationFailed, "Hãy gõ XÓA để xác nhận.",
                new Dictionary<string, string[]> { ["confirmText"] = ["Hãy gõ XÓA để xác nhận."] });
        }

        if (user.PasswordHash is not null && (string.IsNullOrEmpty(req.Password) || !await users.CheckPasswordAsync(user, req.Password)))
        {
            return IdentityErrors.WrongPassword;
        }

        user.DeletedAt = time.GetUtcNow();
        await users.UpdateSecurityStampAsync(user);
        await sessions.RevokeAllAsync(user.Id, RevokeReason.AccountDeleted, client, exceptSessionId: null, ct);
        events.Add(AuthEventKind.AccountDeleted, user.Id, client);
        await db.SaveChangesAsync(ct);
        return Result.Ok();
    }
}

/// <summary>Quản trị người dùng: liệt kê, khóa, mở khóa (FR-ID-08, BR-AUTH-14).</summary>
public sealed class AdminUserHandlers(UserManager<AppUser> users, AppDbContext db, SessionService sessions, TimeProvider time)
{
    public async Task<IReadOnlyList<AdminUserResponse>> ListAsync(string? q, int page, int pageSize, CancellationToken ct)
    {
        var query = db.Users.AsNoTracking().Where(u => u.DeletedAt == null);
        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim().ToUpperInvariant();
            query = query.Where(u => u.NormalizedEmail!.Contains(term) || u.DisplayName.ToUpper().Contains(term));
        }

        var now = time.GetUtcNow();
        var rows = await query.OrderByDescending(u => u.CreatedAt)
            .Skip((Math.Max(page, 1) - 1) * pageSize).Take(pageSize)
            .Select(u => new
            {
                u.Id, u.DisplayName, u.Email, u.EmailConfirmed, u.CreatedAt,
                Locked = u.LockoutEnd != null && u.LockoutEnd > now,
                IsAdmin = db.UserRoles.Any(ur => ur.UserId == u.Id && db.Roles.Any(r => r.Id == ur.RoleId && r.Name == Roles.Admin)),
            })
            .ToListAsync(ct);

        return rows.Select(r => new AdminUserResponse(r.Id, r.DisplayName, r.Email ?? string.Empty, r.EmailConfirmed, r.Locked, r.IsAdmin, r.CreatedAt)).ToList();
    }

    public async Task<Result> LockAsync(Guid targetId, ClientInfo client, CancellationToken ct)
    {
        var user = await users.FindByIdAsync(targetId.ToString());
        if (user is null || user.IsDeleted)
        {
            return Error.NotFound();
        }

        // Không được khóa quản trị viên cuối cùng (kể cả tự khóa mình) - tránh khóa chết hệ thống.
        if (await users.IsInRoleAsync(user, Roles.Admin))
        {
            var activeAdmins = await users.GetUsersInRoleAsync(Roles.Admin);
            var now = time.GetUtcNow();
            if (activeAdmins.Count(a => !a.IsDeleted && !(a.LockoutEnd is { } e && e > now)) <= 1)
            {
                return IdentityErrors.LastAdmin;
            }
        }

        await users.SetLockoutEnabledAsync(user, true);
        await users.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
        await users.UpdateSecurityStampAsync(user);
        await sessions.RevokeAllAsync(user.Id, RevokeReason.AdminLock, client, exceptSessionId: null, ct);
        return Result.Ok();
    }

    public async Task<Result> UnlockAsync(Guid targetId)
    {
        var user = await users.FindByIdAsync(targetId.ToString());
        if (user is null || user.IsDeleted)
        {
            return Error.NotFound();
        }

        await users.SetLockoutEndDateAsync(user, null);
        await users.ResetAccessFailedCountAsync(user);
        return Result.Ok();
    }
}

internal static class AuditJson
{
    public static string Of(object o) => JsonSerializer.Serialize(o);
}
