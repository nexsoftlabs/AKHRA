using System.Security.Claims;

namespace MoviePlatform.Application.Commerce;

public interface ICheckoutService
{
    Task<CheckoutResult> CreateMovieCheckoutAsync(string movieSlug, ClaimsPrincipal user, CancellationToken cancellationToken);

    Task<CheckoutResult> VerifyPaymentAsync(VerifyCheckoutRequest request, ClaimsPrincipal user, CancellationToken cancellationToken);

    Task<bool> ProcessWebhookAsync(string rawBody, string signatureHeader, CancellationToken cancellationToken);
}
