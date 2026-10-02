using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MoviePlatform.Application.Identity;

namespace MoviePlatform.Infrastructure.Identity;

public sealed class EmailOptions
{
    public const string SectionName = "Email";
    public string? SmtpHost { get; set; }
    public int SmtpPort { get; set; } = 587;
    public string? SmtpUser { get; set; }
    public string? SmtpPassword { get; set; }
    public string FromAddress { get; set; } = "noreply@akhra.app";
    public string FromName { get; set; } = "AKHRA";
    public string? PublicWebBaseUrl { get; set; } = "http://localhost:5173";

    public bool IsConfigured => !string.IsNullOrWhiteSpace(SmtpHost);
}

public sealed class SmtpEmailSender(IOptions<EmailOptions> options, ILogger<SmtpEmailSender> logger) : IEmailSender
{
    public async Task SendAsync(string email, string subject, string body, CancellationToken cancellationToken)
    {
        var cfg = options.Value;
        if (!cfg.IsConfigured)
        {
            logger.LogWarning("SMTP not configured; email to {Email} not sent. Subject: {Subject}", email, subject);
            return;
        }

        using var message = new MailMessage
        {
            From = new MailAddress(cfg.FromAddress, cfg.FromName),
            Subject = subject,
            Body = body,
            IsBodyHtml = body.Contains('<', StringComparison.Ordinal),
        };
        message.To.Add(email);

        using var client = new SmtpClient(cfg.SmtpHost, cfg.SmtpPort)
        {
            EnableSsl = cfg.SmtpPort != 25,
            Credentials = string.IsNullOrWhiteSpace(cfg.SmtpUser)
                ? CredentialCache.DefaultNetworkCredentials
                : new NetworkCredential(cfg.SmtpUser, cfg.SmtpPassword),
        };

        await client.SendMailAsync(message, cancellationToken);
    }
}
