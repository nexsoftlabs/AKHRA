using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace MoviePlatform.Infrastructure.Security;

public static class CrossSiteCookiePolicy
{
    public static bool IsCrossOriginSpa(IHostEnvironment environment) =>
        !environment.IsDevelopment()
        && !environment.IsEnvironment("Testing")
        && !environment.IsEnvironment("IntegrationTests");

    public static void Apply(CookieBuilder builder, bool crossOriginSpa)
    {
        if (!crossOriginSpa)
        {
            return;
        }

        builder.SameSite = SameSiteMode.None;
        builder.SecurePolicy = CookieSecurePolicy.Always;
    }

    public static void Apply(CookieOptions options, bool crossOriginSpa)
    {
        if (!crossOriginSpa)
        {
            return;
        }

        options.SameSite = SameSiteMode.None;
        options.Secure = true;
    }
}
