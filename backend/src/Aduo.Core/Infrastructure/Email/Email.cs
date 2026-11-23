using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Aduo.Core.Infrastructure.Email;

public sealed record EmailMessage(string To, string Subject, string TextBody);

public interface IEmailSender
{
    Task SendAsync(EmailMessage message, CancellationToken ct);
}

public sealed class EmailOptions
{
    public const string SectionName = "Email";

    /// <summary>Để trống = không gửi thật, chỉ ghi log (dev). Không bao giờ ghi nội dung chứa link vào log ở production.</summary>
    public string? SmtpHost { get; set; }

    public int SmtpPort { get; set; } = 587;

    public string? SmtpUser { get; set; }

    public string? SmtpPassword { get; set; }

    public string FromAddress { get; set; } = "no-reply@aduo.local";

    public string FromName { get; set; } = "ADUO";

    /// <summary>Chỉ bật ở dev: ghi nội dung email (có link) vào log để thử luồng mà không cần SMTP.</summary>
    public bool LogBodies { get; set; }
}

/// <summary>Gửi qua SMTP bằng MailKit.</summary>
public sealed class SmtpEmailSender(IOptions<EmailOptions> options) : IEmailSender
{
    public async Task SendAsync(EmailMessage message, CancellationToken ct)
    {
        var o = options.Value;
        var mime = new MimeMessage();
        mime.From.Add(new MailboxAddress(o.FromName, o.FromAddress));
        mime.To.Add(MailboxAddress.Parse(message.To));
        mime.Subject = message.Subject;
        mime.Body = new TextPart("plain") { Text = message.TextBody };

        using var client = new SmtpClient();
        await client.ConnectAsync(o.SmtpHost!, o.SmtpPort, SecureSocketOptions.StartTlsWhenAvailable, ct);
        if (!string.IsNullOrEmpty(o.SmtpUser))
        {
            await client.AuthenticateAsync(o.SmtpUser, o.SmtpPassword ?? string.Empty, ct);
        }

        await client.SendAsync(mime, ct);
        await client.DisconnectAsync(true, ct);
    }
}

/// <summary>Không gửi thật. Dùng khi chưa cấu hình SMTP (dev/test).</summary>
public sealed class LoggingEmailSender(ILogger<LoggingEmailSender> logger, IOptions<EmailOptions> options) : IEmailSender
{
    public Task SendAsync(EmailMessage message, CancellationToken ct)
    {
        if (options.Value.LogBodies)
        {
            logger.LogInformation("[DEV EMAIL] To={To} Subject={Subject}\n{Body}", message.To, message.Subject, message.TextBody);
        }
        else
        {
            logger.LogInformation("Email (không gửi thật, chưa cấu hình SMTP): {Subject}", message.Subject);
        }

        return Task.CompletedTask;
    }
}
