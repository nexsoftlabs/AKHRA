using Microsoft.Extensions.Options;
using MoviePlatform.Application.Identity;

namespace MoviePlatform.Infrastructure.Identity;

public sealed class CompositeEmailSender(
    IOptions<EmailOptions> options,
    SmtpEmailSender smtp,
    DevEmailSender dev) : IEmailSender
{
    public Task SendAsync(string email, string subject, string body, CancellationToken cancellationToken) =>
        options.Value.IsConfigured
            ? smtp.SendAsync(email, subject, body, cancellationToken)
            : dev.SendAsync(email, subject, body, cancellationToken);
}
