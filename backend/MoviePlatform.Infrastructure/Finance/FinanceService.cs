using Microsoft.EntityFrameworkCore;
using MoviePlatform.Application.Finance;
using MoviePlatform.Domain.Enums;
using MoviePlatform.Infrastructure.Persistence;

namespace MoviePlatform.Infrastructure.Finance;

public sealed class FinanceService(MoviePlatformDbContext db) : IFinanceService
{
    public async Task<IReadOnlyList<RefundablePaymentDto>> ListRefundablePaymentsAsync(
        string? search,
        int take,
        CancellationToken cancellationToken)
    {
        take = Math.Clamp(take, 1, 200);

        var query = db.Payments
            .AsNoTracking()
            .Where(p => p.Status == PaymentStatus.Captured && p.RazorpayPaymentId != null);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(p => EF.Functions.ILike(p.RazorpayPaymentId ?? "", $"%{term}%"));
        }

        return await query
            .OrderByDescending(p => p.UpdatedAt)
            .Take(take)
            .Join(
                db.Orders,
                p => p.OrderId,
                o => o.Id,
                (p, o) => new { p, o })
            .Join(
                db.Users,
                x => x.o.UserId,
                u => u.Id,
                (x, u) => new RefundablePaymentDto(
                    x.p.Id,
                    x.o.Id,
                    u.Email,
                    x.p.AmountMinorUnits,
                    x.p.Currency,
                    x.p.Status.ToString(),
                    x.p.RazorpayPaymentId,
                    x.p.UpdatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<RefundHistoryDto>> ListRefundsAsync(string? search, int take, CancellationToken cancellationToken)
    {
        take = Math.Clamp(take, 1, 200);
        var query = db.Refunds.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(r => EF.Functions.ILike(r.Reason ?? "", $"%{term}%"));
        }

        return await query
            .OrderByDescending(r => r.CreatedAt)
            .Take(take)
            .Join(db.Payments, r => r.PaymentId, p => p.Id, (r, p) => new { r, p })
            .Join(db.Orders, x => x.p.OrderId, o => o.Id, (x, o) => new { x.r, x.p, o })
            .Join(db.Users, x => x.o.UserId, u => u.Id, (x, u) => new RefundHistoryDto(
                x.r.Id,
                x.r.PaymentId,
                u.Email,
                x.r.AmountMinorUnits,
                x.r.Currency,
                x.r.Status.ToString(),
                x.r.Reason,
                x.r.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}
