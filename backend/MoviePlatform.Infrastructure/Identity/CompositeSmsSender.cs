using Microsoft.Extensions.Options;
using MoviePlatform.Application.Identity;

namespace MoviePlatform.Infrastructure.Identity;

public sealed class CompositeSmsSender(
    IOptions<SmsOptions> options,
    TwilioSmsSender twilio,
    DevSmsSender dev) : ISmsSender
{
    public Task SendAsync(string phoneNumberE164, string message, CancellationToken cancellationToken) =>
        options.Value.IsTwilioConfigured
            ? twilio.SendAsync(phoneNumberE164, message, cancellationToken)
            : dev.SendAsync(phoneNumberE164, message, cancellationToken);
}
