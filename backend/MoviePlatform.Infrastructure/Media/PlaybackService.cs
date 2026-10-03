using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MoviePlatform.Application.Media;
using MoviePlatform.Domain.Enums;
using MoviePlatform.Domain.Entitlements;
using MoviePlatform.Infrastructure.Entitlements;
using MoviePlatform.Infrastructure.Persistence;

namespace MoviePlatform.Infrastructure.Media;

public sealed class PlaybackService(
    MoviePlatformDbContext db,
    IOptions<MediaStorageOptions> mediaOptions,
    ICloudFrontUrlSigner cloudFront) : IPlaybackService
{
    public async Task<PlaybackStartDto?> StartPlaybackAsync(
        string movieSlug,
        ClaimsPrincipal user,
        string? ipAddress,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(user);
        if (userId is null)
        {
            return null;
        }

        var now = DateTimeOffset.UtcNow;
        var movie = await db.Movies
            .AsNoTracking()
            .Include(m => m.MediaAssets)
            .FirstOrDefaultAsync(
                m =>
                    m.Slug == movieSlug &&
                    m.PublicationStatus == PublicationStatus.Published,
                cancellationToken);

        if (movie is null)
        {
            return null;
        }

        var freeVimeo = FreeVimeoAccess.IsFreeVimeoTitle(movie);
        var entitled = freeVimeo || await AccessEvaluator.CanStreamMovieAsync(
            db,
            userId.Value,
            movie.Id,
            movie.SubscriptionEligible,
            cancellationToken);

        if (!entitled)
        {
            return null;
        }

        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        var expires = now.AddHours(4);

        db.PlaybackSessions.Add(new PlaybackSession
        {
            UserId = userId.Value,
            MovieId = movie.Id,
            ExpiresAt = expires,
            TokenHash = HashToken(token),
            CreatedAt = now,
            IpAddress = ipAddress,
        });
        await db.SaveChangesAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(movie.VimeoVideoId))
        {
            return new PlaybackStartDto(
                token,
                expires,
                "vimeo",
                null,
                movie.VimeoVideoId.Trim());
        }

        var manifest = movie.MediaAssets
            .Where(a => a.AssetType == MediaAssetType.HlsManifest && a.IsReady)
            .OrderByDescending(a => a.CreatedAt)
            .FirstOrDefault();

        if (manifest is null)
        {
            return null;
        }

        var manifestUrl = ManifestUrlBuilder.ResolvePlaybackUrl(
            manifest.StorageKey,
            movie.Id,
            token,
            mediaOptions,
            cloudFront,
            expires);

        return new PlaybackStartDto(token, expires, "hls", manifestUrl, null);
    }

    public async Task<bool> ValidateSessionTokenAsync(string sessionToken, Guid movieId, CancellationToken cancellationToken) =>
        await ValidateSessionStatic(db, movieId, sessionToken, cancellationToken);

    public static async Task<bool> ValidateSessionStatic(
        MoviePlatformDbContext db,
        Guid movieId,
        string sessionToken,
        CancellationToken cancellationToken)
    {
        var hash = HashToken(sessionToken);
        var now = DateTimeOffset.UtcNow;
        return await db.PlaybackSessions.AnyAsync(
            s => s.TokenHash == hash && s.MovieId == movieId && s.ExpiresAt > now,
            cancellationToken);
    }

    public async Task<IReadOnlyList<LibraryItemDto>> ListLibraryAsync(
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(user);
        if (userId is null)
        {
            return [];
        }

        var now = DateTimeOffset.UtcNow;
        return await db.Entitlements
            .AsNoTracking()
            .Where(e =>
                e.UserId == userId &&
                e.Status == EntitlementStatus.Active &&
                (e.ExpiresAt == null || e.ExpiresAt > now))
            .Join(
                db.Movies,
                e => e.MovieId,
                m => m.Id,
                (e, m) => new LibraryItemDto(
                    m.Id,
                    m.Title,
                    m.Slug,
                    m.PosterUrl,
                    m.DurationSeconds,
                    e.CreatedAt))
            .OrderByDescending(x => x.PurchasedAt)
            .ToListAsync(cancellationToken);
    }

    public static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes);
    }

    private static Guid? GetUserId(ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var id) ? id : null;
    }
}
