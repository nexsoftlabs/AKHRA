using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MoviePlatform.Application.Identity;

namespace MoviePlatform.Infrastructure.Identity;

public sealed class SmsOptions
{
    public const string SectionName = "Sms";
    public string? ProviderApiKey { get; set; }
    public string? AccountSid { get; set; }
    public string? FromNumber { get; set; }

    public bool IsTwilioConfigured =>
        !string.IsNullOrWhiteSpace(ProviderApiKey) &&
        !string.IsNullOrWhiteSpace(AccountSid) &&
        !string.IsNullOrWhiteSpace(FromNumber);
}

public sealed class TwilioSmsSender(
    HttpClient httpClient,
    IOptions<SmsOptions> options,
    ILogger<TwilioSmsSender> logger) : ISmsSender
{
    public async Task SendAsync(string phoneNumberE164, string message, CancellationToken cancellationToken)
    {
        var cfg = options.Value;
        if (!cfg.IsTwilioConfigured)
        {
            logger.LogWarning("Twilio not configured; SMS to {Phone} not sent: {Message}", phoneNumberE164, message);
            return;
        }

        var url = $"https://api.twilio.com/2010-04-01/Accounts/{cfg.AccountSid}/Messages.json";
        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        var auth = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{cfg.AccountSid}:{cfg.ProviderApiKey}"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", auth);
        request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["To"] = phoneNumberE164,
            ["From"] = cfg.FromNumber!,
            ["Body"] = message,
        });

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            logger.LogError("Twilio SMS failed ({Status}): {Body}", (int)response.StatusCode, body);
        }
    }
}
