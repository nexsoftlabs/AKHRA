using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace MoviePlatform.Application.Identity;

public interface IAuthService
{
    Task<AuthResult> RegisterAsync(RegisterRequest request, HttpContext httpContext, CancellationToken cancellationToken);
    Task<AuthResult> LoginAsync(LoginRequest request, HttpContext httpContext, CancellationToken cancellationToken);
    Task<AuthResult> GoogleSignInAsync(GoogleSignInRequest request, HttpContext httpContext, CancellationToken cancellationToken);
    Task<AuthResult> LinkGoogleAsync(GoogleSignInRequest request, ClaimsPrincipal principal, CancellationToken cancellationToken);
    Task LogoutAsync(HttpContext httpContext, CancellationToken cancellationToken);
    Task<AuthResult> RefreshSessionAsync(HttpContext httpContext, CancellationToken cancellationToken);
    Task RequestPasswordResetAsync(PasswordResetRequest request, CancellationToken cancellationToken);
    Task ConfirmPasswordResetAsync(PasswordResetConfirmRequest request, CancellationToken cancellationToken);
    Task<UserProfileDto?> GetProfileAsync(ClaimsPrincipal principal, CancellationToken cancellationToken);
    Task DeleteAccountAsync(ClaimsPrincipal principal, HttpContext httpContext, CancellationToken cancellationToken);
    Task<AuthResult> ConfirmEmailAsync(ConfirmEmailRequest request, CancellationToken cancellationToken);
    Task ResendEmailConfirmationAsync(ClaimsPrincipal principal, CancellationToken cancellationToken);
}

public interface IOtpAuthService
{
    Task<OtpRequestResult> RequestOtpAsync(OtpRequest request, string? ipAddress, CancellationToken cancellationToken);
    Task<AuthResult> VerifyOtpAsync(OtpVerifyRequest request, HttpContext httpContext, CancellationToken cancellationToken);
}

public interface ISessionService
{
    Task IssueSessionAsync(ApplicationUserRef user, HttpContext httpContext, CancellationToken cancellationToken);
    Task<bool> TryRefreshAsync(HttpContext httpContext, CancellationToken cancellationToken);
    Task RevokeCurrentSessionAsync(HttpContext httpContext, CancellationToken cancellationToken);
}

public record ApplicationUserRef(Guid Id, string? Email, string? UserName);

public interface IGoogleIdTokenValidator
{
    Task<GoogleTokenPayload?> ValidateAsync(string idToken, CancellationToken cancellationToken);
}

public record GoogleTokenPayload(string Subject, string Email, string? Name, bool EmailVerified);

public interface ISmsSender
{
    Task SendAsync(string phoneNumberE164, string message, CancellationToken cancellationToken);
}

public interface IEmailSender
{
    Task SendAsync(string email, string subject, string body, CancellationToken cancellationToken);
}
