using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MoviePlatform.Application.Authorization;
using MoviePlatform.Application.Identity;
using MoviePlatform.Domain.Identity;

namespace MoviePlatform.Infrastructure.Identity;

public sealed class AuthService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    ISessionService sessionService,
    IGoogleIdTokenValidator googleValidator,
    IEmailSender emailSender,
    Microsoft.Extensions.Options.IOptions<EmailOptions> emailOptions,
    Microsoft.Extensions.Options.IOptions<AuthOptions> authOptions) : IAuthService
{
    private const string GoogleLoginProvider = "Google";

    public async Task<AuthResult> RegisterAsync(
        RegisterRequest request,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        request = AuthInputNormalizer.Normalize(request);
        var existing = await userManager.FindByEmailAsync(request.Email);
        if (existing is not null)
        {
            return AuthResult.Fail("email_in_use", "An account with this email already exists.");
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            DisplayName = request.DisplayName,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var createResult = await userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            return AuthResult.Fail("registration_failed", string.Join("; ", createResult.Errors.Select(e => e.Description)));
        }

        await userManager.AddToRoleAsync(user, AppRoles.RegisteredUser);

        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var baseUrl = emailOptions.Value.PublicWebBaseUrl?.TrimEnd('/') ?? "http://localhost:5173";
        var link = $"{baseUrl}/confirm-email?email={Uri.EscapeDataString(user.Email!)}&token={Uri.EscapeDataString(token)}";
        await emailSender.SendAsync(
            user.Email!,
            "Confirm your AKHRA email",
            $"<p>Welcome to AKHRA.</p><p><a href=\"{link}\">Confirm your email</a></p><p>Or use this token in the app: {token}</p>",
            cancellationToken);

        await sessionService.IssueSessionAsync(new ApplicationUserRef(user.Id, user.Email, user.UserName), httpContext, cancellationToken);
        return AuthResult.Ok(await UserProfileMapper.ToProfileAsync(user, userManager));
    }

    public async Task<AuthResult> LoginAsync(
        LoginRequest request,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        request = AuthInputNormalizer.Normalize(request);
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null || user.DeletedAt is not null)
        {
            return AuthResult.Fail("invalid_credentials", "Invalid email or password.");
        }

        var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            return AuthResult.Fail(
                result.IsLockedOut ? "account_locked" : "invalid_credentials",
                result.IsLockedOut ? "Account is temporarily locked." : "Invalid email or password.");
        }

        await sessionService.IssueSessionAsync(new ApplicationUserRef(user.Id, user.Email, user.UserName), httpContext, cancellationToken);
        return AuthResult.Ok(await UserProfileMapper.ToProfileAsync(user, userManager));
    }

    public async Task<AuthResult> GoogleSignInAsync(
        GoogleSignInRequest request,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!IsGoogleConfigured())
        {
            return AuthResult.Fail(
                "google_not_configured",
                "Google sign-in is not configured. Set Auth__GoogleClientId on the API.");
        }

        var payload = await ValidateGooglePayloadAsync(request.IdToken, cancellationToken);
        if (payload is null)
        {
            return AuthResult.Fail("invalid_google_token", "Google sign-in could not be verified.");
        }

        var loginInfo = new UserLoginInfo(GoogleLoginProvider, payload.Subject, GoogleLoginProvider);
        var user = await userManager.FindByLoginAsync(GoogleLoginProvider, payload.Subject);
        if (user is not null)
        {
            if (user.DeletedAt is not null)
            {
                return AuthResult.Fail("account_deleted", "This account has been deleted.");
            }

            await sessionService.IssueSessionAsync(new ApplicationUserRef(user.Id, user.Email, user.UserName), httpContext, cancellationToken);
            return AuthResult.Ok(await UserProfileMapper.ToProfileAsync(user, userManager));
        }

        user = await userManager.FindByEmailAsync(payload.Email);
        if (user is not null)
        {
            if (user.DeletedAt is not null)
            {
                return AuthResult.Fail("account_deleted", "This account has been deleted.");
            }

            var logins = await userManager.GetLoginsAsync(user);
            var hasGoogle = logins.Any(l => l.LoginProvider == GoogleLoginProvider);
            if (!hasGoogle)
            {
                if (await userManager.HasPasswordAsync(user))
                {
                    return AuthResult.Fail(
                        "link_required",
                        "An account with this email exists. Sign in with your password first to link Google.");
                }

                await userManager.AddLoginAsync(user, loginInfo);
            }
            await sessionService.IssueSessionAsync(new ApplicationUserRef(user.Id, user.Email, user.UserName), httpContext, cancellationToken);
            return AuthResult.Ok(await UserProfileMapper.ToProfileAsync(user, userManager));
        }

        user = new ApplicationUser
        {
            UserName = payload.Email,
            Email = payload.Email,
            EmailConfirmed = true,
            DisplayName = payload.Name,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var create = await userManager.CreateAsync(user);
        if (!create.Succeeded)
        {
            return AuthResult.Fail("registration_failed", string.Join("; ", create.Errors.Select(e => e.Description)));
        }

        await userManager.AddToRoleAsync(user, AppRoles.RegisteredUser);
        await userManager.AddLoginAsync(user, loginInfo);
        await sessionService.IssueSessionAsync(new ApplicationUserRef(user.Id, user.Email, user.UserName), httpContext, cancellationToken);
        return AuthResult.Ok(await UserProfileMapper.ToProfileAsync(user, userManager));
    }

    public async Task<AuthResult> LinkGoogleAsync(
        GoogleSignInRequest request,
        ClaimsPrincipal principal,
        CancellationToken cancellationToken)
    {
        if (!IsGoogleConfigured())
        {
            return AuthResult.Fail(
                "google_not_configured",
                "Google sign-in is not configured. Set Auth__GoogleClientId on the API.");
        }

        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null || !Guid.TryParse(userId, out var id))
        {
            return AuthResult.Fail("not_authenticated", "Sign in to link Google.");
        }

        var user = await userManager.Users.FirstOrDefaultAsync(u => u.Id == id && u.DeletedAt == null, cancellationToken);
        if (user is null)
        {
            return AuthResult.Fail("not_authenticated", "Sign in to link Google.");
        }

        var payload = await ValidateGooglePayloadAsync(request.IdToken, cancellationToken);
        if (payload is null)
        {
            return AuthResult.Fail("invalid_google_token", "Google sign-in could not be verified.");
        }

        if (!string.Equals(user.Email, payload.Email, StringComparison.OrdinalIgnoreCase))
        {
            return AuthResult.Fail(
                "email_mismatch",
                "Google account email must match your AKHRA account email.");
        }

        var existing = await userManager.FindByLoginAsync(GoogleLoginProvider, payload.Subject);
        if (existing is not null && existing.Id != user.Id)
        {
            return AuthResult.Fail("google_in_use", "This Google account is already linked to another user.");
        }

        var logins = await userManager.GetLoginsAsync(user);
        if (logins.Any(l => l.LoginProvider == GoogleLoginProvider))
        {
            return AuthResult.Ok(await UserProfileMapper.ToProfileAsync(user, userManager));
        }

        var loginInfo = new UserLoginInfo(GoogleLoginProvider, payload.Subject, GoogleLoginProvider);
        var link = await userManager.AddLoginAsync(user, loginInfo);
        if (!link.Succeeded)
        {
            return AuthResult.Fail("link_failed", string.Join("; ", link.Errors.Select(e => e.Description)));
        }

        if (!user.EmailConfirmed)
        {
            user.EmailConfirmed = true;
            await userManager.UpdateAsync(user);
        }

        return AuthResult.Ok(await UserProfileMapper.ToProfileAsync(user, userManager));
    }

    private bool IsGoogleConfigured() => !string.IsNullOrWhiteSpace(authOptions.Value.GoogleClientId);

    private async Task<GoogleTokenPayload?> ValidateGooglePayloadAsync(string idToken, CancellationToken cancellationToken)
    {
        var payload = await googleValidator.ValidateAsync(idToken, cancellationToken);
        if (payload is null || !payload.EmailVerified)
        {
            return null;
        }

        return payload;
    }

    public Task LogoutAsync(HttpContext httpContext, CancellationToken cancellationToken) =>
        sessionService.RevokeCurrentSessionAsync(httpContext, cancellationToken);

    public async Task<AuthResult> RefreshSessionAsync(HttpContext httpContext, CancellationToken cancellationToken)
    {
        var refreshed = await sessionService.TryRefreshAsync(httpContext, cancellationToken);
        if (!refreshed)
        {
            return AuthResult.Fail("invalid_refresh", "Refresh token is invalid or expired.");
        }

        var principal = httpContext.User;
        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null || !Guid.TryParse(userId, out var id))
        {
            return AuthResult.Fail("invalid_refresh", "Session could not be established.");
        }

        var user = await userManager.FindByIdAsync(id.ToString());
        if (user is null || user.DeletedAt is not null)
        {
            return AuthResult.Fail("invalid_refresh", "User not found.");
        }

        return AuthResult.Ok(await UserProfileMapper.ToProfileAsync(user, userManager));
    }

    public async Task RequestPasswordResetAsync(PasswordResetRequest request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null || user.DeletedAt is not null)
        {
            return;
        }

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        await emailSender.SendAsync(
            user.Email!,
            "Password reset",
            $"Reset token: {token}",
            cancellationToken);
    }

    public async Task ConfirmPasswordResetAsync(PasswordResetConfirmRequest request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null || user.DeletedAt is not null)
        {
            throw new InvalidOperationException("Invalid reset request.");
        }

        var result = await userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));
        }
    }

    public async Task<UserProfileDto?> GetProfileAsync(ClaimsPrincipal principal, CancellationToken cancellationToken)
    {
        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null || !Guid.TryParse(userId, out var id))
        {
            return null;
        }

        var user = await userManager.Users.FirstOrDefaultAsync(u => u.Id == id && u.DeletedAt == null, cancellationToken);
        if (user is null)
        {
            return null;
        }

        return await UserProfileMapper.ToProfileAsync(user, userManager);
    }

    public async Task<AuthResult> ConfirmEmailAsync(ConfirmEmailRequest request, CancellationToken cancellationToken)
    {
        request = new ConfirmEmailRequest(AuthInputNormalizer.NormalizeEmail(request.Email), request.Token);
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null || user.DeletedAt is not null)
        {
            return AuthResult.Fail("invalid_token", "Invalid confirmation link.");
        }

        var result = await userManager.ConfirmEmailAsync(user, request.Token);
        if (!result.Succeeded)
        {
            return AuthResult.Fail("invalid_token", "Invalid or expired confirmation token.");
        }

        return AuthResult.Ok(await UserProfileMapper.ToProfileAsync(user, userManager));
    }

    public async Task ResendEmailConfirmationAsync(ClaimsPrincipal principal, CancellationToken cancellationToken)
    {
        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null || !Guid.TryParse(userId, out var id))
        {
            throw new InvalidOperationException("Not authenticated.");
        }

        var user = await userManager.Users.FirstOrDefaultAsync(u => u.Id == id && u.DeletedAt == null, cancellationToken);
        if (user is null)
        {
            throw new InvalidOperationException("User not found.");
        }

        if (user.EmailConfirmed)
        {
            return;
        }

        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var baseUrl = emailOptions.Value.PublicWebBaseUrl?.TrimEnd('/') ?? "http://localhost:5173";
        var link = $"{baseUrl}/confirm-email?email={Uri.EscapeDataString(user.Email!)}&token={Uri.EscapeDataString(token)}";
        await emailSender.SendAsync(
            user.Email!,
            "Confirm your AKHRA email",
            $"<p>Confirm your email to unlock checkout.</p><p><a href=\"{link}\">Confirm email</a></p>",
            cancellationToken);
    }

    public async Task DeleteAccountAsync(ClaimsPrincipal principal, HttpContext httpContext, CancellationToken cancellationToken)
    {
        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null || !Guid.TryParse(userId, out var id))
        {
            throw new InvalidOperationException("Not authenticated.");
        }

        var user = await userManager.FindByIdAsync(id.ToString());
        if (user is null)
        {
            return;
        }

        user.DeletedAt = DateTimeOffset.UtcNow;
        user.Email = $"deleted+{user.Id:N}@invalid.local";
        user.NormalizedEmail = user.Email.ToUpperInvariant();
        user.UserName = user.Email;
        user.NormalizedUserName = user.NormalizedEmail;
        user.PhoneNumber = null;
        await userManager.UpdateAsync(user);
        await sessionService.RevokeCurrentSessionAsync(httpContext, cancellationToken);
    }
}
