using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using MoviePlatform.Application.Catalog;
using MoviePlatform.Domain.Enums;
using MoviePlatform.Infrastructure.Entitlements;
using MoviePlatform.Infrastructure.Persistence;

namespace MoviePlatform.Infrastructure.Catalog;

public sealed class CatalogService(MoviePlatformDbContext db) : ICatalogService
{
    public async Task<IReadOnlyList<MovieListItemDto>> ListPublishedAsync(
        string? search,
        string? genreSlug,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var query = db.Movies
            .AsNoTracking()
            .Where(m => m.PublicationStatus == PublicationStatus.Published)
            .Where(m => m.AvailabilityStart == null || m.AvailabilityStart <= now)
            .Where(m => m.AvailabilityEnd == null || m.AvailabilityEnd >= now);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(m =>
                EF.Functions.ILike(m.Title, $"%{term}%") ||
                EF.Functions.ILike(m.Slug, $"%{term}%"));
        }

        if (!string.IsNullOrWhiteSpace(genreSlug))
        {
            var slug = genreSlug.Trim().ToLowerInvariant();
            query = query.Where(m => m.MovieGenres.Any(mg => mg.Genre.Slug == slug));
        }

        return await query
            .OrderByDescending(m => m.IsFeatured)
            .ThenBy(m => m.Title)
            .Select(m => new MovieListItemDto(
                m.Id,
                m.Title,
                m.Slug,
                m.PosterUrl,
                m.ReleaseYear,
                m.DurationSeconds,
                m.AgeRating,
                m.PriceMinorUnits,
                m.Currency,
                m.IsFeatured,
                m.MovieGenres.Select(mg => mg.Genre.Name).ToList()))
            .ToListAsync(cancellationToken);
    }

    public async Task<MovieDetailDto?> GetPublishedBySlugAsync(
        string slug,
        ClaimsPrincipal? user,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var movie = await db.Movies
            .AsNoTracking()
            .Include(m => m.MovieGenres)
            .ThenInclude(mg => mg.Genre)
            .FirstOrDefaultAsync(m =>
                m.Slug == slug &&
                m.PublicationStatus == PublicationStatus.Published &&
                (m.AvailabilityStart == null || m.AvailabilityStart <= now) &&
                (m.AvailabilityEnd == null || m.AvailabilityEnd >= now),
                cancellationToken);

        if (movie is null)
        {
            return null;
        }

        var hasAccess = false;
        if (user?.Identity?.IsAuthenticated == true)
        {
            if (FreeVimeoAccess.IsFreeVimeoTitle(movie))
            {
                hasAccess = true;
            }
            else
            {
                var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (Guid.TryParse(userIdClaim, out var userId))
                {
                    hasAccess = await AccessEvaluator.CanStreamMovieAsync(
                        db,
                        userId,
                        movie.Id,
                        movie.SubscriptionEligible,
                        cancellationToken);
                }
            }
        }

        return new MovieDetailDto(
            movie.Id,
            movie.Title,
            movie.Slug,
            movie.Description,
            movie.Synopsis,
            movie.Language,
            movie.ReleaseYear,
            movie.DurationSeconds,
            movie.AgeRating,
            movie.PosterUrl,
            movie.BackdropUrl,
            movie.TrailerUrl,
            movie.PriceMinorUnits,
            movie.Currency,
            movie.PurchaseType.ToString(),
            movie.SubscriptionEligible,
            movie.IsFeatured,
            movie.MovieGenres.Select(mg => mg.Genre.Name).ToList(),
            hasAccess,
            movie.VimeoVideoId);
    }

    public async Task<IReadOnlyList<string>> ListPublishedGenreSlugsAsync(CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        return await db.MovieGenres
            .AsNoTracking()
            .Where(mg =>
                mg.Movie.PublicationStatus == PublicationStatus.Published &&
                (mg.Movie.AvailabilityStart == null || mg.Movie.AvailabilityStart <= now) &&
                (mg.Movie.AvailabilityEnd == null || mg.Movie.AvailabilityEnd >= now))
            .Select(mg => mg.Genre.Slug)
            .Distinct()
            .OrderBy(s => s)
            .ToListAsync(cancellationToken);
    }
}
