using Microsoft.Extensions.Logging;
using MoviePlatform.Application.Identity;

namespace MoviePlatform.Infrastructure.Identity;

public sealed class DevEmailSender(ILogger<DevEmailSender> logger) : IEmailSender
{
    public static string? LastEmail { get; private set; }
    public static string? LastSubject { get; private set; }
    public static string? LastBody { get; private set; }

    public Task SendAsync(string email, string subject, string body, CancellationToken cancellationToken)
    {
        LastEmail = email;
        LastSubject = subject;
        LastBody = body;
        logger.LogInformation("DEV email to {Email} subject {Subject}", email, subject);
        return Task.CompletedTask;
    }
}
