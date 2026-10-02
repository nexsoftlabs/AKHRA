using System.Security.Claims;

namespace MoviePlatform.Application.Media;

public interface IPlaybackService
{
    Task<PlaybackStartDto?> StartPlaybackAsync(string movieSlug, ClaimsPrincipal user, string? ipAddress, CancellationToken cancellationToken);

    Task<bool> ValidateSessionTokenAsync(string sessionToken, Guid movieId, CancellationToken cancellationToken);

    Task<IReadOnlyList<LibraryItemDto>> ListLibraryAsync(ClaimsPrincipal user, CancellationToken cancellationToken);
}
