using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using MoviePlatform.Application.Identity;

namespace MoviePlatform.Infrastructure.Identity;

public sealed class DevSmsSender(ILogger<DevSmsSender> logger) : ISmsSender
{
    private static readonly ConcurrentDictionary<string, string> LastMessages = new();

    public static string? GetLastMessageFor(string phoneE164) =>
        LastMessages.TryGetValue(phoneE164, out var msg) ? msg : null;

    public Task SendAsync(string phoneNumberE164, string message, CancellationToken cancellationToken)
    {
        LastMessages[phoneNumberE164] = message;
        logger.LogInformation("DEV SMS to {Phone}: {Message}", phoneNumberE164, message);
        return Task.CompletedTask;
    }
}
