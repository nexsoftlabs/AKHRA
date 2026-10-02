using Microsoft.EntityFrameworkCore;
using MoviePlatform.Application.Admin;
using MoviePlatform.Domain.Enums;
using MoviePlatform.Infrastructure.Persistence;

namespace MoviePlatform.Infrastructure.Admin;

public sealed class AdminDashboardService(MoviePlatformDbContext db) : IAdminDashboardService
{
    public async Task<AdminDashboardStatsDto> GetStatsAsync(CancellationToken cancellationToken)
    {
        var since30 = DateTimeOffset.UtcNow.AddDays(-30);

        var totalUsers = await db.Users.CountAsync(u => u.DeletedAt == null, cancellationToken);
        var publishedMovies = await db.Movies.CountAsync(
            m => m.PublicationStatus == PublicationStatus.Published,
            cancellationToken);
        var draftMovies = await db.Movies.CountAsync(
            m => m.PublicationStatus != PublicationStatus.Published,
            cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var activeSubscriptions = await db.Subscriptions.CountAsync(
            s =>
                s.Status == SubscriptionStatus.Active &&
                s.CurrentPeriodEnd != null &&
                s.CurrentPeriodEnd > now,
            cancellationToken);

        var payments30 = await db.Payments
            .Where(p => p.Status == PaymentStatus.Captured && p.UpdatedAt >= since30)
            .ToListAsync(cancellationToken);

        var revenue30 = payments30.Sum(p => p.AmountMinorUnits);
        var refunds30 = await db.Refunds.CountAsync(r => r.CreatedAt >= since30, cancellationToken);

        var recent = await db.Payments
            .AsNoTracking()
            .OrderByDescending(p => p.UpdatedAt)
            .Take(8)
            .Join(db.Orders, p => p.OrderId, o => o.Id, (p, o) => new { p, o })
            .Join(db.Users, x => x.o.UserId, u => u.Id, (x, u) => new AdminRecentPaymentDto(
                x.p.Id,
                u.Email,
                x.p.AmountMinorUnits,
                x.p.Currency,
                x.p.Status.ToString(),
                x.p.UpdatedAt))
            .ToListAsync(cancellationToken);

        return new AdminDashboardStatsDto(
            totalUsers,
            publishedMovies,
            draftMovies,
            activeSubscriptions,
            revenue30,
            payments30.Count,
            refunds30,
            recent);
    }
}
