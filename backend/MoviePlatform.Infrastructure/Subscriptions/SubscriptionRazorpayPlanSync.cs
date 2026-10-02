using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MoviePlatform.Domain.Enums;
using MoviePlatform.Infrastructure.Commerce;
using MoviePlatform.Infrastructure.Persistence;

namespace MoviePlatform.Infrastructure.Subscriptions;

/// <summary>Creates Razorpay billing plans for local subscription_plans when keys are configured.</summary>
public sealed class SubscriptionRazorpayPlanSync(
    IServiceScopeFactory scopeFactory,
    IOptions<RazorpayOptions> razorpayOptions,
    ILogger<SubscriptionRazorpayPlanSync> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!razorpayOptions.Value.IsConfigured)
        {
            return;
        }

        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<MoviePlatformDbContext>();
        var razorpay = scope.ServiceProvider.GetRequiredService<IRazorpayApiClient>();

        var plans = await db.SubscriptionPlans
            .Where(p => p.IsActive && p.RazorpayPlanId == null)
            .ToListAsync(cancellationToken);

        foreach (var plan in plans)
        {
            try
            {
                var (period, periodInterval) = BillingIntervalExtensions.ToRazorpay(plan.BillingInterval);
                plan.RazorpayPlanId = await razorpay.EnsurePlanAsync(
                    plan.Name,
                    plan.PriceMinorUnits,
                    period,
                    periodInterval,
                    cancellationToken);
                logger.LogInformation("Synced Razorpay plan for {Slug}: {PlanId}", plan.Slug, plan.RazorpayPlanId);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Could not sync Razorpay plan for {Slug}", plan.Slug);
            }
        }

        if (plans.Count > 0)
        {
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
