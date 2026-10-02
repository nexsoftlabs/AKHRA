using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MoviePlatform.Application.Commerce;
using MoviePlatform.Application.Subscriptions;
using MoviePlatform.Domain.Commerce;
using MoviePlatform.Domain.Enums;
using MoviePlatform.Domain.Entitlements;
using MoviePlatform.Domain.Identity;
using MoviePlatform.Infrastructure.Commerce;
using MoviePlatform.Infrastructure.Persistence;

namespace MoviePlatform.Infrastructure.Subscriptions;

public sealed class SubscriptionService(
    MoviePlatformDbContext db,
    IRazorpayApiClient razorpay,
    IOptions<RazorpayOptions> razorpayOptions,
    UserManager<ApplicationUser> userManager) : ISubscriptionService
{
    public async Task<IReadOnlyList<SubscriptionPlanDto>> ListPlansAsync(CancellationToken cancellationToken)
    {
        var plans = await db.SubscriptionPlans
            .AsNoTracking()
            .Where(p => p.IsActive)
            .OrderBy(p => p.BillingInterval)
            .ThenBy(p => p.PriceMinorUnits)
            .ToListAsync(cancellationToken);

        return plans
            .Select(p => new SubscriptionPlanDto(
                p.Id,
                p.Name,
                p.Slug,
                p.Description,
                p.PriceMinorUnits,
                p.Currency,
                p.BillingInterval.ToString(),
                BillingIntervalExtensions.PeriodMonths(p.BillingInterval),
                BillingIntervalExtensions.DisplayLabel(p.BillingInterval)))
            .ToList();
    }

    public async Task<CheckoutResult> CreatePlanCheckoutAsync(
        string planSlug,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        if (!razorpayOptions.Value.IsConfigured)
        {
            return CheckoutResult.Fail("razorpay_not_configured", "Payments are not configured.");
        }

        var userId = GetUserId(user);
        if (userId is null)
        {
            return CheckoutResult.Fail("unauthorized", "Sign in to subscribe.");
        }

        var account = await userManager.FindByIdAsync(userId.Value.ToString());
        if (account is null || !account.EmailConfirmed)
        {
            return CheckoutResult.Fail("email_not_confirmed", "Confirm your email before subscribing.");
        }

        var plan = await db.SubscriptionPlans.FirstOrDefaultAsync(p => p.Slug == planSlug && p.IsActive, cancellationToken);
        if (plan is null)
        {
            return CheckoutResult.Fail("plan_not_found", "Subscription plan not found.");
        }

        var order = new Order
        {
            UserId = userId.Value,
            Status = OrderStatus.AwaitingPayment,
            TotalMinorUnits = plan.PriceMinorUnits,
            Currency = plan.Currency,
            CorrelationId = Guid.CreateVersion7().ToString("N"),
        };

        order.Items.Add(new OrderItem
        {
            OrderId = order.Id,
            SubscriptionPlanId = plan.Id,
            Description = plan.Name,
            UnitPriceMinorUnits = plan.PriceMinorUnits,
            Quantity = 1,
        });

        var payment = new Payment
        {
            OrderId = order.Id,
            Status = PaymentStatus.Pending,
            AmountMinorUnits = plan.PriceMinorUnits,
            Currency = plan.Currency,
            IdempotencyKey = $"plan-{userId}-{plan.Id}-{Guid.CreateVersion7():N}",
        };

        order.Payments.Add(payment);
        db.Orders.Add(order);
        await db.SaveChangesAsync(cancellationToken);

        try
        {
            string? razorpaySubId = null;
            if (!string.IsNullOrWhiteSpace(plan.RazorpayPlanId))
            {
                try
                {
                    razorpaySubId = await razorpay.CreateSubscriptionAsync(plan.RazorpayPlanId, 120, cancellationToken);
                }
                catch
                {
                    razorpaySubId = null;
                }
            }

            if (!string.IsNullOrWhiteSpace(razorpaySubId))
            {
                payment.RazorpayOrderId = razorpaySubId;
                db.Subscriptions.Add(new Subscription
                {
                    UserId = userId.Value,
                    PlanId = plan.Id,
                    Status = SubscriptionStatus.Pending,
                    ProviderSubscriptionId = razorpaySubId,
                });
                await db.SaveChangesAsync(cancellationToken);

                return CheckoutResult.Ok(new CreateCheckoutResponse(
                    payment.Id,
                    string.Empty,
                    razorpayOptions.Value.KeyId,
                    plan.PriceMinorUnits,
                    plan.Currency,
                    plan.Name,
                    razorpaySubId));
            }

            var receipt = order.Id.ToString("N")[..24];
            var razorpayOrderId = await razorpay.CreateOrderAsync(
                plan.PriceMinorUnits,
                plan.Currency,
                receipt,
                new Dictionary<string, string>
                {
                    ["plan_id"] = plan.Id.ToString(),
                    ["user_id"] = userId.Value.ToString(),
                    ["payment_id"] = payment.Id.ToString(),
                },
                cancellationToken);

            payment.RazorpayOrderId = razorpayOrderId;
            await db.SaveChangesAsync(cancellationToken);

            return CheckoutResult.Ok(new CreateCheckoutResponse(
                payment.Id,
                razorpayOrderId,
                razorpayOptions.Value.KeyId,
                plan.PriceMinorUnits,
                plan.Currency,
                plan.Name));
        }
        catch (Exception ex)
        {
            order.Status = OrderStatus.Failed;
            payment.Status = PaymentStatus.Failed;
            await db.SaveChangesAsync(cancellationToken);
            return CheckoutResult.Fail("razorpay_error", ex.Message);
        }
    }

    public Task<bool> HasActiveSubscriptionAsync(Guid userId, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        return db.Subscriptions.AnyAsync(
            s =>
                s.UserId == userId &&
                s.Status == SubscriptionStatus.Active &&
                s.CurrentPeriodEnd != null &&
                s.CurrentPeriodEnd > now,
            cancellationToken);
    }

    private static Guid? GetUserId(ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var id) ? id : null;
    }
}
