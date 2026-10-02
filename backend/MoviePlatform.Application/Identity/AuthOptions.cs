namespace MoviePlatform.Application.Identity;

public class AuthOptions
{
    public const string SectionName = "Auth";

    public string GoogleClientId { get; set; } = string.Empty;
    public int AccessCookieHours { get; set; } = 8;
    public int RefreshTokenDays { get; set; } = 14;
    public int OtpExpiryMinutes { get; set; } = 5;
    public int OtpMaxAttempts { get; set; } = 5;
    public int OtpRequestCooldownSeconds { get; set; } = 60;
    public int OtpMaxRequestsPerHourPerPhone { get; set; } = 5;
    public string RefreshCookieName { get; set; } = "mp.refresh";
}
