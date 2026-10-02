using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using MoviePlatform.Application.Authorization;
using MoviePlatform.Application.Subscriptions;
using MoviePlatform.Domain.Commerce;
using MoviePlatform.Domain.Enums;
using MoviePlatform.Domain.Entitlements;
using MoviePlatform.Infrastructure.Commerce;
using MoviePlatform.Infrastructure.Persistence;

namespace MoviePlatform.Infrastructure.Subscriptions;

public sealed class RefundService(
    MoviePlatformDbContext db,
    IRazorpayApiClient razorpay) : IRefundService
{
    public async Task<RefundDto> RequestRefundAsync(
        RefundRequestDto request,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        if (!user.IsInRole(AppRoles.FinanceAdmin) &&
            !user.IsInRole(AppRoles.PlatformAdmin) &&
            !user.IsInRole(AppRoles.SuperAdmin))
        {
            throw new UnauthorizedAccessException();
        }

        var payment = await db.Payments
            .Include(p => p.Order)
            .FirstOrDefaultAsync(p => p.Id == request.PaymentId, cancellationToken);

        if (payment is null || payment.Status != PaymentStatus.Captured)
        {
            throw new InvalidOperationException("Payment is not eligible for refund.");
        }

        if (string.IsNullOrWhiteSpace(payment.RazorpayPaymentId))
        {
            throw new InvalidOperationException("Payment has no provider reference.");
        }

        var refundAmount = request.AmountMinorUnits ?? payment.AmountMinorUnits;
        if (refundAmount <= 0 || refundAmount > payment.AmountMinorUnits)
        {
            throw new InvalidOperationException("Invalid refund amount.");
        }

        var providerRefundId = await razorpay.CreateRefundAsync(
            payment.RazorpayPaymentId,
            refundAmount,
            cancellationToken);

        var refund = new Refund
        {
            PaymentId = payment.Id,
            Status = RefundStatus.Processed,
            AmountMinorUnits = refundAmount,
            Currency = payment.Currency,
            Reason = request.Reason,
            ProviderRefundId = providerRefundId,
            RequestedByUserId = GetUserId(user),
        };

        payment.Status = refundAmount >= payment.AmountMinorUnits
            ? PaymentStatus.Refunded
            : PaymentStatus.Captured;

        if (refundAmount >= payment.AmountMinorUnits)
        {
            await RevokeEntitlementsForOrderAsync(payment.OrderId, cancellationToken);
            await RevokeSubscriptionForOrderAsync(payment.OrderId, cancellationToken);
        }

        db.Refunds.Add(refund);
        await db.SaveChangesAsync(cancellationToken);

        return new RefundDto(
            refund.Id,
            refund.PaymentId,
            refund.Status.ToString(),
            refund.AmountMinorUnits,
            refund.Currency,
            refund.ProviderRefundId);
    }

    private async Task RevokeEntitlementsForOrderAsync(Guid orderId, CancellationToken cancellationToken)
    {
        var movieIds = await db.OrderItems
            .Where(i => i.OrderId == orderId && i.MovieId != null)
            .Select(i => i.MovieId!.Value)
            .ToListAsync(cancellationToken);

        if (movieIds.Count == 0)
        {
            return;
        }

        var userId = await db.Orders.Where(o => o.Id == orderId).Select(o => o.UserId).FirstAsync(cancellationToken);

        var entitlements = await db.Entitlements
            .Where(e => e.UserId == userId && movieIds.Contains(e.MovieId) && e.Status == EntitlementStatus.Active)
            .ToListAsync(cancellationToken);

        foreach (var e in entitlements)
        {
            e.Status = EntitlementStatus.Revoked;
        }
    }

    private async Task RevokeSubscriptionForOrderAsync(Guid orderId, CancellationToken cancellationToken)
    {
        var planId = await db.OrderItems
            .Where(i => i.OrderId == orderId && i.SubscriptionPlanId != null)
            .Select(i => i.SubscriptionPlanId!.Value)
            .FirstOrDefaultAsync(cancellationToken);

        if (planId == Guid.Empty)
        {
            return;
        }

        var userId = await db.Orders.Where(o => o.Id == orderId).Select(o => o.UserId).FirstAsync(cancellationToken);
        var subs = await db.Subscriptions
            .Where(s => s.UserId == userId && s.PlanId == planId && s.Status == SubscriptionStatus.Active)
            .ToListAsync(cancellationToken);

        foreach (var sub in subs)
        {
            sub.Status = SubscriptionStatus.Cancelled;
            sub.CancelledAt = DateTimeOffset.UtcNow;
        }
    }

    private static Guid? GetUserId(ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var id) ? id : null;
    }
}
