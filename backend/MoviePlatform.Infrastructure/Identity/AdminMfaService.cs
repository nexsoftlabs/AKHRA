using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using MoviePlatform.Application.Authorization;
using MoviePlatform.Domain.Identity;

namespace MoviePlatform.Infrastructure.Identity;

public sealed class AdminMfaOptions
{
    public const string SectionName = "AdminMfa";
    public string CookieName { get; set; } = "mp.admin-mfa";
    public int StepUpHours { get; set; } = 12;
    public bool SkipStepUpInDevelopment { get; set; } = true;
}

public sealed class AdminMfaService(
    UserManager<ApplicationUser> userManager,
    IOptions<AdminMfaOptions> options)
{
    private static readonly string[] AdminRoles =
    [
        AppRoles.ContentManager,
        AppRoles.SupportAgent,
        AppRoles.FinanceAdmin,
        AppRoles.PlatformAdmin,
        AppRoles.SuperAdmin,
    ];

    public async Task<bool> RequiresStepUpAsync(ClaimsPrincipal principal)
    {
        if (!principal.Identity?.IsAuthenticated ?? true)
        {
            return false;
        }

        var roles = await GetRolesAsync(principal);
        return roles.Any(r => AdminRoles.Contains(r));
    }

    public async Task<MfaSetupDto?> GetSetupAsync(ClaimsPrincipal principal)
    {
        var user = await GetUserAsync(principal);
        if (user is null)
        {
            return null;
        }

        var key = await userManager.GetAuthenticatorKeyAsync(user);
        if (string.IsNullOrEmpty(key))
        {
            await userManager.ResetAuthenticatorKeyAsync(user);
            key = await userManager.GetAuthenticatorKeyAsync(user);
        }

        var email = user.Email ?? user.UserName ?? "user";
        var uri = $"otpauth://totp/AKHRA:{Uri.EscapeDataString(email)}?secret={key}&issuer=AKHRA&digits=6";
        return new MfaSetupDto(uri, user.TwoFactorEnabled);
    }

    public async Task<bool> ConfirmSetupAsync(ClaimsPrincipal principal, string code)
    {
        var user = await GetUserAsync(principal);
        if (user is null)
        {
            return false;
        }

        var valid = await userManager.VerifyTwoFactorTokenAsync(
            user,
            userManager.Options.Tokens.AuthenticatorTokenProvider,
            code);

        if (!valid)
        {
            return false;
        }

        await userManager.SetTwoFactorEnabledAsync(user, true);
        return true;
    }

    public async Task<bool> VerifyStepUpAsync(ClaimsPrincipal principal, string code, HttpContext httpContext)
    {
        var user = await GetUserAsync(principal);
        if (user is null || !user.TwoFactorEnabled)
        {
            return false;
        }

        var valid = await userManager.VerifyTwoFactorTokenAsync(
            user,
            userManager.Options.Tokens.AuthenticatorTokenProvider,
            code);

        if (!valid)
        {
            return false;
        }

        IssueStepUpCookie(httpContext);
        return true;
    }

    public bool HasValidStepUpCookie(HttpContext httpContext) =>
        httpContext.Request.Cookies.TryGetValue(options.Value.CookieName, out var value) &&
        value == "verified";

    public void IssueStepUpCookie(HttpContext httpContext)
    {
        httpContext.Response.Cookies.Append(
            options.Value.CookieName,
            "verified",
            new CookieOptions
            {
                HttpOnly = true,
                Secure = httpContext.Request.IsHttps,
                SameSite = SameSiteMode.Strict,
                MaxAge = TimeSpan.FromHours(options.Value.StepUpHours),
                Path = "/",
            });
    }

    private async Task<ApplicationUser?> GetUserAsync(ClaimsPrincipal principal)
    {
        var id = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (id is null)
        {
            return null;
        }

        return await userManager.FindByIdAsync(id);
    }

    private static async Task<IReadOnlyList<string>> GetRolesAsync(ClaimsPrincipal principal)
    {
        if (principal is ClaimsPrincipal p && p.Identity?.IsAuthenticated == true)
        {
            return p.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        }

        return [];
    }
}

public record MfaSetupDto(string AuthenticatorUri, bool IsEnabled);
