using Microsoft.AspNetCore.Antiforgery;
using Microsoft.Extensions.Hosting;
using MoviePlatform.Infrastructure.Security;

namespace MoviePlatform.Api.Security;

public static class AntiforgeryExtensions
{
    public const string HeaderName = "X-XSRF-TOKEN";
    public const string CookieName = "XSRF-TOKEN";

    public static IServiceCollection AddSpaAntiforgery(this IServiceCollection services, IHostEnvironment environment)
    {
        var crossOriginSpa = CrossSiteCookiePolicy.IsCrossOriginSpa(environment);
        services.AddAntiforgery(options =>
        {
            options.HeaderName = HeaderName;
            options.Cookie.Name = CookieName;
            options.Cookie.HttpOnly = false;
            options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            options.Cookie.SameSite = crossOriginSpa ? SameSiteMode.None : SameSiteMode.Strict;
            CrossSiteCookiePolicy.Apply(options.Cookie, crossOriginSpa);
        });
        return services;
    }

    public static RouteGroupBuilder MapAntiforgeryEndpoint(this RouteGroupBuilder api)
    {
        api.MapGet("/antiforgery/token", (IAntiforgery antiforgery, HttpContext context) =>
        {
            var tokens = antiforgery.GetAndStoreTokens(context);
            return Results.Ok(new { token = tokens.RequestToken });
        }).AllowAnonymous();
        return api;
    }

    public static IApplicationBuilder UseApiAntiforgery(this IApplicationBuilder app)
    {
        return app.UseWhen(
            static context =>
            {
                var env = context.RequestServices.GetRequiredService<IHostEnvironment>();
                return !env.IsEnvironment("Testing") && !env.IsEnvironment("IntegrationTests");
            },
            static branch => branch.UseMiddleware<AntiforgeryValidationMiddleware>());
    }
}
