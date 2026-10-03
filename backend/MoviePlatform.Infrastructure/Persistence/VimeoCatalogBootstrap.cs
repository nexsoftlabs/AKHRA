using Microsoft.EntityFrameworkCore;
using MoviePlatform.Domain.Catalog;
using MoviePlatform.Domain.Enums;

namespace MoviePlatform.Infrastructure.Persistence;

/// <summary>Upserts licensed Vimeo catalog titles for all environments.</summary>
public static class VimeoCatalogBootstrap
{
    private static readonly (string Slug, string Title, string VimeoId, string Synopsis, bool Featured)[] Titles =
    [
        (
            "the-hunt-english",
            "The Hunt (English)",
            "1231958324",
            "The Hunt — English.",
            true),
        (
            "gadi-lohardaga-mail",
            "Gadi Lohardaga Mail",
            "1231959016",
            "Gadi Lohardaga Mail.",
            true),
    ];

    public static async Task EnsureAsync(MoviePlatformDbContext db, CancellationToken cancellationToken = default)
    {
        var drama = await db.Genres.FirstOrDefaultAsync(g => g.Slug == "drama", cancellationToken);
        if (drama is null)
        {
            drama = new Genre { Name = "Drama", Slug = "drama" };
            db.Genres.Add(drama);
            await db.SaveChangesAsync(cancellationToken);
        }

        foreach (var seed in Titles)
        {
            var movie = await db.Movies
                .Include(m => m.MovieGenres)
                .FirstOrDefaultAsync(m => m.Slug == seed.Slug, cancellationToken);

            var poster = $"https://vumbnail.com/{seed.VimeoId}.jpg";

            if (movie is null)
            {
                movie = new Movie
                {
                    Title = seed.Title,
                    Slug = seed.Slug,
                    Description = seed.Title,
                    Synopsis = seed.Synopsis,
                    Language = "en",
                    ReleaseYear = 2025,
                    DurationSeconds = 7200,
                    AgeRating = "U/A 13+",
                    PosterUrl = poster,
                    BackdropUrl = poster,
                    VimeoVideoId = seed.VimeoId,
                    PriceMinorUnits = 0,
                    Currency = "INR",
                    PurchaseType = PurchaseType.Lifetime,
                    SubscriptionEligible = false,
                    PublicationStatus = PublicationStatus.Published,
                    AvailabilityStart = DateTimeOffset.UtcNow.AddYears(-1),
                    LicenseTerritories = ["IN"],
                    IsFeatured = seed.Featured,
                    MovieGenres = [new MovieGenre { Genre = drama }],
                };

                movie.License = new MovieLicense
                {
                    Movie = movie,
                    RightsHolder = "AKHRA",
                    LicenseReference = $"VIMEO-{seed.Slug}",
                    ValidFrom = DateTimeOffset.UtcNow.AddYears(-1),
                    ValidTo = DateTimeOffset.UtcNow.AddYears(5),
                    Territories = ["IN"],
                    AllowsStreaming = true,
                };

                db.Movies.Add(movie);
            }
            else
            {
                movie.Title = seed.Title;
                movie.Synopsis = seed.Synopsis;
                movie.VimeoVideoId = seed.VimeoId;
                movie.PosterUrl = poster;
                movie.BackdropUrl = poster;
                movie.PublicationStatus = PublicationStatus.Published;
                movie.PriceMinorUnits = 0;
                movie.IsFeatured = seed.Featured;
            }
        }

        var keepSlugs = Titles.Select(t => t.Slug).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var mockTitles = await db.Movies
            .Where(m =>
                m.PublicationStatus == PublicationStatus.Published &&
                (string.IsNullOrEmpty(m.VimeoVideoId) || !keepSlugs.Contains(m.Slug)))
            .ToListAsync(cancellationToken);

        foreach (var mock in mockTitles)
        {
            mock.PublicationStatus = PublicationStatus.Archived;
            mock.IsFeatured = false;
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
