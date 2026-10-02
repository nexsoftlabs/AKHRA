using Microsoft.EntityFrameworkCore;
using MoviePlatform.Domain.Enums;
using MoviePlatform.Domain.Entitlements;

namespace MoviePlatform.Infrastructure.Persistence;

public static class SubscriptionSeeder
{
    private sealed record PlanSeed(
        string Slug,
        string Name,
        string Description,
        long PriceMinorUnits,
        BillingInterval BillingInterval);

    private static readonly PlanSeed[] DefaultPlans =
    [
        new(
            "monthly",
            "Monthly",
            "All subscription-eligible titles. Billed every month.",
            19900,
            BillingInterval.Monthly),
        new(
            "3-months",
            "3 Months",
            "Save vs monthly. Full catalog access for 90 days.",
            49900,
            BillingInterval.ThreeMonths),
        new(
            "6-months",
            "6 Months",
            "Best value for regular viewers. Six months of streaming.",
            89900,
            BillingInterval.SixMonths),
        new(
            "yearly",
            "1 Year",
            "Twelve months of all-access streaming at the lowest effective rate.",
            159900,
            BillingInterval.Annual),
    ];

    public static async Task SeedAsync(MoviePlatformDbContext db, CancellationToken cancellationToken = default)
    {
        foreach (var seed in DefaultPlans)
        {
            var existing = await db.SubscriptionPlans.FirstOrDefaultAsync(p => p.Slug == seed.Slug, cancellationToken);
            if (existing is null)
            {
                db.SubscriptionPlans.Add(new SubscriptionPlan
                {
                    Name = seed.Name,
                    Slug = seed.Slug,
                    Description = seed.Description,
                    PriceMinorUnits = seed.PriceMinorUnits,
                    Currency = "INR",
                    BillingInterval = seed.BillingInterval,
                    IsActive = true,
                    GracePeriodDays = 3,
                });
            }
            else
            {
                existing.Name = seed.Name;
                existing.Description = seed.Description;
                existing.PriceMinorUnits = seed.PriceMinorUnits;
                existing.BillingInterval = seed.BillingInterval;
                existing.IsActive = true;
            }
        }

        var legacy = await db.SubscriptionPlans
            .FirstOrDefaultAsync(p => p.Slug == "monthly-all-access", cancellationToken);
        if (legacy is not null)
        {
            legacy.IsActive = false;
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
