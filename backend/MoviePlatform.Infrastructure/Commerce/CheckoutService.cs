using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MoviePlatform.Domain.Identity;
using Microsoft.Extensions.Options;
using MoviePlatform.Application.Commerce;
using MoviePlatform.Domain.Commerce;
using MoviePlatform.Domain.Enums;
using MoviePlatform.Domain.Entitlements;
using MoviePlatform.Infrastructure.Persistence;

namespace MoviePlatform.Infrastructure.Commerce;

public sealed class CheckoutService(
    MoviePlatformDbContext db,
    IRazorpayApiClient razorpay,
    IOptions<RazorpayOptions> razorpayOptions,
    UserManager<ApplicationUser> userManager) : ICheckoutService
{
    public async Task<CheckoutResult> CreateMovieCheckoutAsync(
        string movieSlug,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        if (!razorpayOptions.Value.IsConfigured)
        {
            return CheckoutResult.Fail(
                "razorpay_not_configured",
                "Payments are not configured. Add Razorpay test KeyId and KeySecret to the API (see .env.example).");
        }

        var userId = GetUserId(user);
        if (userId is null)
        {
            return CheckoutResult.Fail("unauthorized", "Sign in to purchase.");
        }

        var account = await userManager.FindByIdAsync(userId.Value.ToString());
        if (account is null || !account.EmailConfirmed)
        {
            return CheckoutResult.Fail("email_not_confirmed", "Confirm your email before purchasing.");
        }

        var now = DateTimeOffset.UtcNow;
        var movie = await db.Movies
            .AsNoTracking()
            .FirstOrDefaultAsync(
                m =>
                    m.Slug == movieSlug &&
                    m.PublicationStatus == PublicationStatus.Published &&
                    (m.AvailabilityStart == null || m.AvailabilityStart <= now) &&
                    (m.AvailabilityEnd == null || m.AvailabilityEnd >= now),
                cancellationToken);

        if (movie is null)
        {
            return CheckoutResult.Fail("movie_not_found", "Movie is not available for purchase.");
        }

        var hasAccess = await db.Entitlements.AnyAsync(
            e =>
                e.UserId == userId &&
                e.MovieId == movie.Id &&
                e.Status == EntitlementStatus.Active &&
                (e.ExpiresAt == null || e.ExpiresAt > now),
            cancellationToken);

        if (hasAccess)
        {
            return CheckoutResult.Fail("already_owned", "You already own this title.");
        }

        var order = new Order
        {
            UserId = userId.Value,
            Status = OrderStatus.AwaitingPayment,
            TotalMinorUnits = movie.PriceMinorUnits,
            Currency = movie.Currency,
            CorrelationId = Guid.CreateVersion7().ToString("N"),
        };

        var orderItem = new OrderItem
        {
            OrderId = order.Id,
            MovieId = movie.Id,
            Description = movie.Title,
            UnitPriceMinorUnits = movie.PriceMinorUnits,
            Quantity = 1,
        };

        var payment = new Payment
        {
            OrderId = order.Id,
            Status = PaymentStatus.Pending,
            AmountMinorUnits = movie.PriceMinorUnits,
            Currency = movie.Currency,
            IdempotencyKey = $"movie-{userId}-{movie.Id}-{Guid.CreateVersion7():N}",
        };

        order.Items.Add(orderItem);
        order.Payments.Add(payment);
        db.Orders.Add(order);
        await db.SaveChangesAsync(cancellationToken);

        try
        {
            var receipt = order.Id.ToString("N")[..24];
            var razorpayOrderId = await razorpay.CreateOrderAsync(
                movie.PriceMinorUnits,
                movie.Currency,
                receipt,
                new Dictionary<string, string>
                {
                    ["movie_id"] = movie.Id.ToString(),
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
                movie.PriceMinorUnits,
                movie.Currency,
                movie.Title));
        }
        catch (Exception ex)
        {
            order.Status = OrderStatus.Failed;
            payment.Status = PaymentStatus.Failed;
            await db.SaveChangesAsync(cancellationToken);
            return CheckoutResult.Fail("razorpay_error", ex.Message);
        }
    }

    public async Task<CheckoutResult> VerifyPaymentAsync(
        VerifyCheckoutRequest request,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(user);
        if (userId is null)
        {
            return CheckoutResult.Fail("unauthorized", "Sign in to complete purchase.");
        }

        var secret = razorpayOptions.Value.KeySecret;
        var signatureOk = !string.IsNullOrWhiteSpace(request.RazorpaySubscriptionId)
            ? RazorpaySignature.VerifySubscriptionPayment(
                request.RazorpaySubscriptionId,
                request.RazorpayPaymentId,
                request.RazorpaySignature,
                secret)
            : !string.IsNullOrWhiteSpace(request.RazorpayOrderId) &&
              RazorpaySignature.VerifyPayment(
                  request.RazorpayOrderId,
                  request.RazorpayPaymentId,
                  request.RazorpaySignature,
                  secret);

        if (!signatureOk)
        {
            return CheckoutResult.Fail("invalid_signature", "Payment verification failed.");
        }

        var remoteStatus = await razorpay.GetPaymentStatusAsync(request.RazorpayPaymentId, cancellationToken);
        if (remoteStatus is not null && remoteStatus is not "captured" and not "authorized")
        {
            return CheckoutResult.Fail("payment_not_captured", $"Payment status is {remoteStatus}.");
        }

        var lookupId = !string.IsNullOrWhiteSpace(request.RazorpaySubscriptionId)
            ? request.RazorpaySubscriptionId
            : request.RazorpayOrderId;

        var payment = await db.Payments
            .Include(p => p.Order)
            .ThenInclude(o => o.Items)
            .FirstOrDefaultAsync(p => p.RazorpayOrderId == lookupId, cancellationToken);

        if (payment is null)
        {
            return CheckoutResult.Fail("payment_not_found", "Unknown order.");
        }

        if (payment.Order.UserId != userId)
        {
            return CheckoutResult.Fail("forbidden", "This payment does not belong to your account.");
        }

        if (!payment.Order.Items.Any(i => i.MovieId is not null || i.SubscriptionPlanId is not null))
        {
            return CheckoutResult.Fail("invalid_order", "Order has no fulfillable items.");
        }

        var movieId = payment.Order.Items.FirstOrDefault(i => i.MovieId is not null)?.MovieId;

        if (payment.Status == PaymentStatus.Captured)
        {
            return CheckoutResult.Verified(new VerifyCheckoutResponse(true, "Already processed.", movieId, true));
        }

        await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);

        payment.RazorpayPaymentId = request.RazorpayPaymentId;
        var now = DateTimeOffset.UtcNow;

        db.PaymentEvents.Add(new PaymentEvent
        {
            PaymentId = payment.Id,
            ProviderEventId = $"client-verify-{request.RazorpayPaymentId}",
            EventType = "payment.captured.client",
            RawPayload = JsonSerializer.Serialize(request),
            ReceivedAt = now,
            Processed = true,
        });

        await PaymentFulfillment.FulfillCapturedPaymentAsync(db, payment, cancellationToken);
        await tx.CommitAsync(cancellationToken);

        var message = movieId is not null
            ? "Purchase complete. You can stream this title."
            : "Subscription active. Stream eligible titles from your library.";
        return CheckoutResult.Verified(new VerifyCheckoutResponse(true, message, movieId, false));
    }

    public async Task<bool> ProcessWebhookAsync(string rawBody, string signatureHeader, CancellationToken cancellationToken)
    {
        var secret = razorpayOptions.Value.WebhookSecret;
        if (string.IsNullOrWhiteSpace(secret))
        {
            return false;
        }

        if (!RazorpaySignature.VerifyWebhook(rawBody, signatureHeader, secret))
        {
            return false;
        }

        using var doc = JsonDocument.Parse(rawBody);
        var root = doc.RootElement;
        var eventType = root.GetProperty("event").GetString();
        if (eventType == "subscription.halted" || eventType == "subscription.cancelled")
        {
            return await HandleSubscriptionLifecycleWebhookAsync(root, eventType!, providerEventId: root.GetProperty("id").GetString(), cancellationToken);
        }

        if (eventType == "subscription.charged")
        {
            return await HandleSubscriptionChargedWebhookAsync(root, cancellationToken);
        }

        if (eventType != "payment.captured")
        {
            return true;
        }

        var providerEventId = root.GetProperty("id").GetString() ?? Guid.CreateVersion7().ToString();
        var paymentEntity = root.GetProperty("payload").GetProperty("payment").GetProperty("entity");
        var razorpayOrderId = paymentEntity.GetProperty("order_id").GetString();
        var razorpayPaymentId = paymentEntity.GetProperty("id").GetString();
        if (string.IsNullOrWhiteSpace(razorpayOrderId) || string.IsNullOrWhiteSpace(razorpayPaymentId))
        {
            return false;
        }

        if (await db.PaymentEvents.AnyAsync(e => e.ProviderEventId == providerEventId, cancellationToken))
        {
            return true;
        }

        var payment = await db.Payments
            .Include(p => p.Order)
            .ThenInclude(o => o.Items)
            .FirstOrDefaultAsync(p => p.RazorpayOrderId == razorpayOrderId, cancellationToken);

        if (payment is null)
        {
            return false;
        }

        await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);

        db.PaymentEvents.Add(new PaymentEvent
        {
            PaymentId = payment.Id,
            ProviderEventId = providerEventId,
            EventType = eventType!,
            RawPayload = rawBody,
            ReceivedAt = DateTimeOffset.UtcNow,
            Processed = true,
        });

        if (payment.Status != PaymentStatus.Captured)
        {
            payment.RazorpayPaymentId = razorpayPaymentId;
            await PaymentFulfillment.FulfillCapturedPaymentAsync(db, payment, cancellationToken);
        }
        else
        {
            await db.SaveChangesAsync(cancellationToken);
        }

        await tx.CommitAsync(cancellationToken);
        return true;
    }

    private async Task<bool> HandleSubscriptionChargedWebhookAsync(JsonElement root, CancellationToken cancellationToken)
    {
        var subEntity = root.GetProperty("payload").GetProperty("subscription").GetProperty("entity");
        var subId = subEntity.GetProperty("id").GetString();
        if (string.IsNullOrWhiteSpace(subId))
        {
            return false;
        }

        var subscription = await db.Subscriptions
            .Include(s => s.Plan)
            .FirstOrDefaultAsync(s => s.ProviderSubscriptionId == subId, cancellationToken);

        if (subscription is null)
        {
            return false;
        }

        var now = DateTimeOffset.UtcNow;
        var periodEnd = now.AddMonths(BillingIntervalExtensions.PeriodMonths(subscription.Plan.BillingInterval));

        subscription.Status = SubscriptionStatus.Active;
        subscription.CurrentPeriodStart = now;
        subscription.CurrentPeriodEnd = periodEnd;
        subscription.BillingEvents.Add(new SubscriptionBillingEvent
        {
            EventType = "subscription.charged",
            ProviderEventId = root.GetProperty("id").GetString(),
            OccurredAt = now,
        });

        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<bool> HandleSubscriptionLifecycleWebhookAsync(
        JsonElement root,
        string eventType,
        string? providerEventId,
        CancellationToken cancellationToken)
    {
        var subEntity = root.GetProperty("payload").GetProperty("subscription").GetProperty("entity");
        var subId = subEntity.GetProperty("id").GetString();
        if (string.IsNullOrWhiteSpace(subId))
        {
            return false;
        }

        var subscription = await db.Subscriptions
            .FirstOrDefaultAsync(s => s.ProviderSubscriptionId == subId, cancellationToken);

        if (subscription is null)
        {
            return true;
        }

        subscription.Status = eventType == "subscription.halted"
            ? SubscriptionStatus.PastDue
            : SubscriptionStatus.Cancelled;
        if (eventType == "subscription.cancelled")
        {
            subscription.CancelledAt = DateTimeOffset.UtcNow;
        }

        subscription.BillingEvents.Add(new SubscriptionBillingEvent
        {
            EventType = eventType,
            ProviderEventId = providerEventId,
            OccurredAt = DateTimeOffset.UtcNow,
        });

        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static Guid? GetUserId(ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var id) ? id : null;
    }
}
