namespace MoviePlatform.Application.Identity;

public record RegisterRequest(string Email, string Password, string? DisplayName);

public record LoginRequest(string Email, string Password);

public record GoogleSignInRequest(string IdToken);

public record OtpRequest(string PhoneNumber);

public record OtpVerifyRequest(string PhoneNumber, string Code);

public record PasswordResetRequest(string Email);

public record PasswordResetConfirmRequest(string Email, string Token, string NewPassword);

public record UserProfileDto(
    Guid Id,
    string? Email,
    string? DisplayName,
    string? PhoneNumber,
    bool EmailConfirmed,
    bool IsPhoneVerified,
    IReadOnlyList<string> Roles,
    bool TwoFactorEnabled,
    bool IsAdmin);

public record ConfirmEmailRequest(string Email, string Token);

public record AuthResult(bool Succeeded, UserProfileDto? User, string? ErrorCode, string? ErrorMessage)
{
    public static AuthResult Ok(UserProfileDto user) => new(true, user, null, null);
    public static AuthResult Fail(string code, string message) => new(false, null, code, message);
}

public record OtpRequestResult(bool Succeeded, string? ErrorCode, string? ErrorMessage, int? RetryAfterSeconds)
{
    public static OtpRequestResult Ok() => new(true, null, null, null);
    public static OtpRequestResult Fail(string code, string message, int? retryAfter = null) =>
        new(false, code, message, retryAfter);
}
