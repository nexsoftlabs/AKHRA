using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MoviePlatform.Application.Authorization;
using MoviePlatform.Domain.Identity;
using MoviePlatform.Infrastructure.Media;

namespace MoviePlatform.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider services, IHostEnvironment environment)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<MoviePlatformDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<MoviePlatformDbContext>>();

        await db.Database.MigrateAsync();

        await SeedRolesAsync(scope.ServiceProvider);
        await SubscriptionSeeder.SeedAsync(db);

        if (!environment.IsDevelopment())
        {
            return;
        }

        await SeedDevUserAsync(scope.ServiceProvider, logger);
        await SeedDevAdminAsync(scope.ServiceProvider, logger);
        await CatalogSeeder.SeedAsync(db);
        var mediaOptions = scope.ServiceProvider.GetRequiredService<IOptions<MediaStorageOptions>>().Value;
        await MediaSeeder.SeedAsync(db, mediaOptions);
        logger.LogInformation("Development database migrated and roles seeded.");
    }

    private static async Task SeedDevUserAsync(IServiceProvider services, ILogger logger)
    {
        const string demoEmail = "demo@akhra.app";
        const string demoPassword = "DemoPass123!";

        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        if (await userManager.FindByEmailAsync(demoEmail) is not null)
        {
            return;
        }

        var user = new ApplicationUser
        {
            UserName = demoEmail,
            Email = demoEmail,
            DisplayName = "Demo Viewer",
            EmailConfirmed = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var result = await userManager.CreateAsync(user, demoPassword);
        if (!result.Succeeded)
        {
            logger.LogWarning("Dev demo user could not be created: {Errors}",
                string.Join("; ", result.Errors.Select(e => e.Description)));
            return;
        }

        await userManager.AddToRoleAsync(user, AppRoles.RegisteredUser);
        logger.LogInformation("Dev demo user seeded: {Email}", demoEmail);
    }

    private static async Task SeedDevAdminAsync(IServiceProvider services, ILogger logger)
    {
        const string adminEmail = "admin@akhra.app";
        const string adminPassword = "AdminPass123!";

        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.FindByEmailAsync(adminEmail);
        if (user is not null)
        {
            if (!await userManager.IsInRoleAsync(user, AppRoles.FinanceAdmin))
            {
                await userManager.AddToRoleAsync(user, AppRoles.FinanceAdmin);
            }

            return;
        }

        user = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            DisplayName = "Catalog Admin",
            EmailConfirmed = true,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        var result = await userManager.CreateAsync(user, adminPassword);
        if (!result.Succeeded)
        {
            logger.LogWarning("Dev admin user could not be created: {Errors}",
                string.Join("; ", result.Errors.Select(e => e.Description)));
            return;
        }

        await userManager.AddToRoleAsync(user, AppRoles.ContentManager);
        await userManager.AddToRoleAsync(user, AppRoles.PlatformAdmin);
        await userManager.AddToRoleAsync(user, AppRoles.FinanceAdmin);
        logger.LogInformation("Dev admin user seeded: {Email}", adminEmail);
    }

    private static async Task SeedRolesAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();

        foreach (var roleName in AppRoles.All)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new ApplicationRole
                {
                    Name = roleName,
                    NormalizedName = roleName.ToUpperInvariant(),
                    Description = $"Platform role: {roleName}"
                });
            }
        }
    }
}
