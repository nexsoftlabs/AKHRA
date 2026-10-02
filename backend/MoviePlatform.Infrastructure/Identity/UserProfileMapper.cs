using Microsoft.AspNetCore.Identity;
using MoviePlatform.Application.Authorization;
using MoviePlatform.Application.Identity;
using MoviePlatform.Domain.Identity;

namespace MoviePlatform.Infrastructure.Identity;

public static class UserProfileMapper
{
    private static readonly string[] AdminRoles =
    [
        AppRoles.ContentManager,
        AppRoles.SupportAgent,
        AppRoles.FinanceAdmin,
        AppRoles.PlatformAdmin,
        AppRoles.SuperAdmin,
    ];

    public static async Task<UserProfileDto> ToProfileAsync(
        ApplicationUser user,
        UserManager<ApplicationUser> userManager)
    {
        var roles = await userManager.GetRolesAsync(user);
        return new UserProfileDto(
            user.Id,
            user.Email,
            user.DisplayName,
            user.PhoneNumber,
            user.EmailConfirmed,
            user.IsPhoneVerified,
            roles.ToList(),
            user.TwoFactorEnabled,
            roles.Any(r => AdminRoles.Contains(r)));
    }
}
