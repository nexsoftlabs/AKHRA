namespace MoviePlatform.Application.Identity;

public static class AuthInputNormalizer
{
    public static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    public static RegisterRequest Normalize(RegisterRequest request) =>
        request with { Email = NormalizeEmail(request.Email) };

    public static LoginRequest Normalize(LoginRequest request) =>
        request with { Email = NormalizeEmail(request.Email) };

    public static PasswordResetRequest Normalize(PasswordResetRequest request) =>
        request with { Email = NormalizeEmail(request.Email) };

    public static PasswordResetConfirmRequest Normalize(PasswordResetConfirmRequest request) =>
        request with { Email = NormalizeEmail(request.Email) };
}
