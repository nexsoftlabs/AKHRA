using System.Security.Claims;
using MoviePlatform.Application.Commerce;

namespace MoviePlatform.Application.Subscriptions;

public interface ISubscriptionService
{
    Task<IReadOnlyList<SubscriptionPlanDto>> ListPlansAsync(CancellationToken cancellationToken);

    Task<CheckoutResult> CreatePlanCheckoutAsync(string planSlug, ClaimsPrincipal user, CancellationToken cancellationToken);

    Task<bool> HasActiveSubscriptionAsync(Guid userId, CancellationToken cancellationToken);
}
