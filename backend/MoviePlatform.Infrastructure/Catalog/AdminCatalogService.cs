using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using MoviePlatform.Application.Catalog;
using MoviePlatform.Domain.Catalog;
using MoviePlatform.Domain.Enums;
using MoviePlatform.Infrastructure.Persistence;

namespace MoviePlatform.Infrastructure.Catalog;

public sealed partial class AdminCatalogService(MoviePlatformDbContext db) : IAdminCatalogService
{
    public async Task<IReadOnlyList<AdminMovieListItemDto>> ListAsync(
        string? search,
        string? status,
        CancellationToken cancellationToken)
    {
        var query = db.Movies.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(m =>
                EF.Functions.ILike(m.Title, $"%{term}%") ||
                EF.Functions.ILike(m.Slug, $"%{term}%"));
        }

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<PublicationStatus>(status, true, out var pub))
        {
            query = query.Where(m => m.PublicationStatus == pub);
        }

        return await query
            .OrderByDescending(m => m.UpdatedAt)
            .Select(m => new AdminMovieListItemDto(
                m.Id,
                m.Title,
                m.Slug,
                m.PublicationStatus,
                m.PriceMinorUnits,
                m.Currency,
                m.UpdatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<AdminMovieDetailDto?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var movie = await db.Movies
            .AsNoTracking()
            .Include(m => m.MovieGenres)
            .ThenInclude(mg => mg.Genre)
            .Include(m => m.License)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

        return movie is null ? null : MapDetail(movie);
    }

    public async Task<AdminMovieDetailDto> CreateAsync(
        CreateMovieRequest request,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var slug = string.IsNullOrWhiteSpace(request.Slug)
            ? Slugify(request.Title)
            : Slugify(request.Slug);

        if (await db.Movies.AnyAsync(m => m.Slug == slug, cancellationToken))
        {
            throw new InvalidOperationException("A movie with this slug already exists.");
        }

        Guid? createdBy = null;
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (Guid.TryParse(userId, out var uid))
        {
            createdBy = uid;
        }

        var movie = new Movie
        {
            Title = request.Title.Trim(),
            Slug = slug,
            Description = request.Description,
            Synopsis = request.Synopsis,
            Language = request.Language,
            ReleaseYear = request.ReleaseYear,
            DurationSeconds = request.DurationSeconds,
            AgeRating = request.AgeRating,
            PosterUrl = request.PosterUrl,
            BackdropUrl = request.BackdropUrl,
            TrailerUrl = request.TrailerUrl,
            PriceMinorUnits = request.PriceMinorUnits,
            Currency = request.Currency,
            PurchaseType = request.PurchaseType,
            SubscriptionEligible = request.SubscriptionEligible,
            PublicationStatus = PublicationStatus.Draft,
            IsFeatured = request.IsFeatured,
            CreatedByUserId = createdBy,
        };

        await ApplyGenresAsync(movie, request.Genres, cancellationToken);
        ApplyLicense(movie, request.License);

        db.Movies.Add(movie);
        await db.SaveChangesAsync(cancellationToken);

        return MapDetail(movie);
    }

    public async Task<AdminMovieDetailDto?> UpdateAsync(
        Guid id,
        UpdateMovieRequest request,
        CancellationToken cancellationToken)
    {
        var movie = await db.Movies
            .Include(m => m.MovieGenres)
            .ThenInclude(mg => mg.Genre)
            .Include(m => m.License)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

        if (movie is null)
        {
            return null;
        }

        var slug = Slugify(request.Slug);
        if (slug != movie.Slug && await db.Movies.AnyAsync(m => m.Slug == slug && m.Id != id, cancellationToken))
        {
            throw new InvalidOperationException("Slug is already in use.");
        }

        movie.Title = request.Title.Trim();
        movie.Slug = slug;
        movie.Description = request.Description;
        movie.Synopsis = request.Synopsis;
        movie.Language = request.Language;
        movie.ReleaseYear = request.ReleaseYear;
        movie.DurationSeconds = request.DurationSeconds;
        movie.AgeRating = request.AgeRating;
        movie.PosterUrl = request.PosterUrl;
        movie.BackdropUrl = request.BackdropUrl;
        movie.TrailerUrl = request.TrailerUrl;
        movie.PriceMinorUnits = request.PriceMinorUnits;
        movie.Currency = request.Currency;
        movie.PurchaseType = request.PurchaseType;
        movie.SubscriptionEligible = request.SubscriptionEligible;
        movie.PublicationStatus = request.PublicationStatus;
        movie.AvailabilityStart = request.AvailabilityStart;
        movie.AvailabilityEnd = request.AvailabilityEnd;
        movie.LicenseTerritories = request.LicenseTerritories;
        movie.IsFeatured = request.IsFeatured;

        movie.MovieGenres.Clear();
        await ApplyGenresAsync(movie, request.Genres, cancellationToken);
        ApplyLicense(movie, request.License);

        await db.SaveChangesAsync(cancellationToken);
        return MapDetail(movie);
    }

    public async Task<bool> PublishAsync(Guid id, CancellationToken cancellationToken)
    {
        var movie = await db.Movies
            .Include(m => m.License)
            .Include(m => m.MediaAssets)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

        if (movie is null)
        {
            return false;
        }

        if (movie.License is null || !movie.License.AllowsStreaming)
        {
            throw new InvalidOperationException("Movie requires a valid license before publishing.");
        }

        var hasStream = movie.MediaAssets.Any(a =>
            a.AssetType == MediaAssetType.HlsManifest && a.IsReady);

        if (!hasStream)
        {
            throw new InvalidOperationException("Movie requires a ready HLS manifest before publishing.");
        }

        movie.PublicationStatus = PublicationStatus.Published;
        if (movie.AvailabilityStart is null)
        {
            movie.AvailabilityStart = DateTimeOffset.UtcNow;
        }

        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> UnpublishAsync(Guid id, CancellationToken cancellationToken)
    {
        var movie = await db.Movies.FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
        if (movie is null)
        {
            return false;
        }

        movie.PublicationStatus = PublicationStatus.Unpublished;
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<IReadOnlyList<string>> ListGenreSlugsAsync(CancellationToken cancellationToken) =>
        await db.Genres.AsNoTracking().OrderBy(g => g.Name).Select(g => g.Slug).ToListAsync(cancellationToken);

    private async Task ApplyGenresAsync(Movie movie, IReadOnlyList<string> genreSlugs, CancellationToken cancellationToken)
    {
        foreach (var raw in genreSlugs.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var slug = Slugify(raw);
            var genre = await db.Genres.FirstOrDefaultAsync(g => g.Slug == slug, cancellationToken);
            if (genre is null)
            {
                genre = new Genre { Name = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(slug.Replace('-', ' ')), Slug = slug };
                db.Genres.Add(genre);
            }

            movie.MovieGenres.Add(new MovieGenre { Genre = genre });
        }
    }

    private static void ApplyLicense(Movie movie, MovieLicenseDto? license)
    {
        if (license is null)
        {
            return;
        }

        movie.License ??= new MovieLicense { Movie = movie };
        movie.License.RightsHolder = license.RightsHolder;
        movie.License.LicenseReference = license.LicenseReference;
        movie.License.ValidFrom = license.ValidFrom;
        movie.License.ValidTo = license.ValidTo;
        movie.License.Territories = license.Territories;
        movie.License.AllowsStreaming = license.AllowsStreaming;
        movie.License.Notes = license.Notes;
    }

    private static AdminMovieDetailDto MapDetail(Movie movie) =>
        new(
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
            movie.PurchaseType,
            movie.SubscriptionEligible,
            movie.PublicationStatus,
            movie.AvailabilityStart,
            movie.AvailabilityEnd,
            movie.LicenseTerritories,
            movie.IsFeatured,
            movie.MovieGenres.Select(mg => mg.Genre.Name).ToList(),
            movie.License is null
                ? null
                : new MovieLicenseDto(
                    movie.License.RightsHolder,
                    movie.License.LicenseReference,
                    movie.License.ValidFrom,
                    movie.License.ValidTo,
                    movie.License.Territories,
                    movie.License.AllowsStreaming,
                    movie.License.Notes));

    private static string Slugify(string value)
    {
        var normalized = value.Trim().ToLowerInvariant();
        normalized = NonSlugChars().Replace(normalized, "-");
        normalized = MultiDash().Replace(normalized, "-").Trim('-');
        return string.IsNullOrEmpty(normalized) ? "movie" : normalized;
    }

    [GeneratedRegex(@"[^a-z0-9\-]+", RegexOptions.Compiled)]
    private static partial Regex NonSlugChars();

    [GeneratedRegex(@"-{2,}", RegexOptions.Compiled)]
    private static partial Regex MultiDash();
}
