using System.Security.Claims;

namespace MoviePlatform.Application.Subscriptions;

public interface IRefundService
{
    Task<RefundDto> RequestRefundAsync(RefundRequestDto request, ClaimsPrincipal user, CancellationToken cancellationToken);
}
