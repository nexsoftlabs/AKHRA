using Microsoft.EntityFrameworkCore;
using MoviePlatform.Domain.Enums;
using MoviePlatform.Infrastructure.Persistence;

namespace MoviePlatform.Infrastructure.Entitlements;

public static class AccessEvaluator
{
    public static async Task<bool> CanStreamMovieAsync(
        MoviePlatformDbContext db,
        Guid userId,
        Guid movieId,
        bool subscriptionEligible,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;

        var entitled = await db.Entitlements.AnyAsync(
            e =>
                e.UserId == userId &&
                e.MovieId == movieId &&
                e.Status == EntitlementStatus.Active &&
                (e.ExpiresAt == null || e.ExpiresAt > now),
            cancellationToken);

        if (entitled)
        {
            return true;
        }

        if (!subscriptionEligible)
        {
            return false;
        }

        return await db.Subscriptions.AnyAsync(
            s =>
                s.UserId == userId &&
                s.Status == SubscriptionStatus.Active &&
                s.CurrentPeriodEnd != null &&
                s.CurrentPeriodEnd > now,
            cancellationToken);
    }
}
