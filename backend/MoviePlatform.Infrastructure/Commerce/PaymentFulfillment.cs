using Microsoft.EntityFrameworkCore;
using MoviePlatform.Domain.Commerce;
using MoviePlatform.Domain.Enums;
using MoviePlatform.Domain.Entitlements;
using MoviePlatform.Infrastructure.Persistence;

namespace MoviePlatform.Infrastructure.Commerce;

public static class PaymentFulfillment
{
    public static async Task<Guid?> FulfillCapturedPaymentAsync(
        MoviePlatformDbContext db,
        Payment payment,
        CancellationToken cancellationToken)
    {
        if (payment.Status == PaymentStatus.Captured)
        {
            var existingMovie = payment.Order.Items.FirstOrDefault(i => i.MovieId is not null)?.MovieId;
            return existingMovie;
        }

        payment.Status = PaymentStatus.Captured;
        payment.Order.Status = OrderStatus.Paid;
        var userId = payment.Order.UserId;
        var now = DateTimeOffset.UtcNow;
        Guid? primaryMovieId = null;

        foreach (var item in payment.Order.Items)
        {
            if (item.MovieId is not null)
            {
                primaryMovieId = item.MovieId;
                await GrantPurchaseAsync(db, userId, item.MovieId.Value, payment, cancellationToken);
            }
            else if (item.SubscriptionPlanId is not null)
            {
                await GrantSubscriptionAsync(db, userId, item.SubscriptionPlanId.Value, payment, now, cancellationToken);
            }
        }

        await db.SaveChangesAsync(cancellationToken);
        return primaryMovieId;
    }

    private static async Task GrantPurchaseAsync(
        MoviePlatformDbContext db,
        Guid userId,
        Guid movieId,
        Payment payment,
        CancellationToken cancellationToken)
    {
        var exists = await db.Entitlements.AnyAsync(
            e => e.UserId == userId && e.MovieId == movieId && e.Status == EntitlementStatus.Active,
            cancellationToken);

        if (exists)
        {
            return;
        }

        var purchase = new Purchase
        {
            UserId = userId,
            MovieId = movieId,
            OrderId = payment.OrderId,
            AmountPaidMinorUnits = payment.AmountMinorUnits,
            Currency = payment.Currency,
        };
        db.Purchases.Add(purchase);

        db.Entitlements.Add(new Entitlement
        {
            UserId = userId,
            MovieId = movieId,
            Source = EntitlementSource.Purchase,
            Status = EntitlementStatus.Active,
            PurchaseId = purchase.Id,
        });
    }

    private static async Task GrantSubscriptionAsync(
        MoviePlatformDbContext db,
        Guid userId,
        Guid planId,
        Payment payment,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var plan = await db.SubscriptionPlans.FirstAsync(p => p.Id == planId, cancellationToken);
        var periodEnd = now.AddMonths(BillingIntervalExtensions.PeriodMonths(plan.BillingInterval));

        var pending = await db.Subscriptions
            .Where(s => s.UserId == userId && s.PlanId == planId && s.Status == SubscriptionStatus.Pending)
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (pending is not null)
        {
            pending.Status = SubscriptionStatus.Active;
            pending.CurrentPeriodStart = now;
            pending.CurrentPeriodEnd = periodEnd;
            pending.BillingEvents.Add(new SubscriptionBillingEvent
            {
                EventType = "payment.captured",
                ProviderEventId = payment.RazorpayPaymentId,
                AmountMinorUnits = payment.AmountMinorUnits,
                Currency = payment.Currency,
                OccurredAt = now,
            });
            return;
        }

        var existing = await db.Subscriptions
            .Where(s => s.UserId == userId && s.PlanId == planId && s.Status == SubscriptionStatus.Active)
            .FirstOrDefaultAsync(cancellationToken);

        if (existing is not null)
        {
            existing.CurrentPeriodEnd = periodEnd;
            existing.CurrentPeriodStart = now;
            return;
        }

        var subscription = new Subscription
        {
            UserId = userId,
            PlanId = planId,
            Status = SubscriptionStatus.Active,
            CurrentPeriodStart = now,
            CurrentPeriodEnd = periodEnd,
        };
        subscription.BillingEvents.Add(new SubscriptionBillingEvent
        {
            EventType = "payment.captured",
            ProviderEventId = payment.RazorpayPaymentId,
            AmountMinorUnits = payment.AmountMinorUnits,
            Currency = payment.Currency,
            OccurredAt = now,
        });
        db.Subscriptions.Add(subscription);
    }
}
