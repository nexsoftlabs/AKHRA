using Microsoft.EntityFrameworkCore;
using MoviePlatform.Domain.Catalog;
using MoviePlatform.Domain.Enums;

namespace MoviePlatform.Infrastructure.Persistence;

public static class CatalogSeeder
{
    public static async Task SeedAsync(MoviePlatformDbContext db, CancellationToken cancellationToken = default)
    {
        if (await db.Movies.AnyAsync(m => m.PublicationStatus == PublicationStatus.Published, cancellationToken))
        {
            return;
        }

        var genres = new Dictionary<string, Genre>
        {
            ["drama"] = new Genre { Name = "Drama", Slug = "drama" },
            ["romance"] = new Genre { Name = "Romance", Slug = "romance" },
            ["thriller"] = new Genre { Name = "Thriller", Slug = "thriller" },
            ["sci-fi"] = new Genre { Name = "Sci-Fi", Slug = "sci-fi" },
        };

        db.Genres.AddRange(genres.Values);

        var movies = new[]
        {
            CreateMovie(
                "Midnight Monsoon",
                "midnight-monsoon",
                "Drama",
                genres["drama"],
                2024,
                6840,
                "U/A 13+",
                5000,
                "https://images.unsplash.com/photo-1478720568477-152d9b164e26?w=600&h=900&fit=crop",
                "https://www.youtube.com/watch?v=dQw4w9WgXcQ",
                true,
                "A family reckons with secrets during the heaviest monsoon in decades."),
            CreateMovie(
                "Coastal Letters",
                "coastal-letters",
                "Romance",
                genres["romance"],
                2023,
                5520,
                "U",
                5000,
                "https://images.unsplash.com/photo-1485846234645-a62644f84728?w=600&h=900&fit=crop",
                "https://www.youtube.com/watch?v=dQw4w9WgXcQ",
                true,
                "Two strangers exchange letters along a windswept shoreline."),
            CreateMovie(
                "The Last Reel",
                "the-last-reel",
                "Thriller",
                genres["thriller"],
                2025,
                7200,
                "A",
                5000,
                "https://images.unsplash.com/photo-1536440136628-849c177e76a1?w=600&h=900&fit=crop",
                "https://www.youtube.com/watch?v=dQw4w9WgXcQ",
                false,
                "An archivist discovers footage that was never meant to be seen."),
            CreateMovie(
                "City of Echoes",
                "city-of-echoes",
                "Sci-Fi",
                genres["sci-fi"],
                2022,
                8100,
                "U/A 16+",
                5000,
                "https://images.unsplash.com/photo-1440404653325-ab127d49abc1?w=600&h=900&fit=crop",
                "https://www.youtube.com/watch?v=dQw4w9WgXcQ",
                false,
                "In a vertical metropolis, sound can be stolen — and sold."),
        };

        db.Movies.AddRange(movies);
        await db.SaveChangesAsync(cancellationToken);
    }

    private static Movie CreateMovie(
        string title,
        string slug,
        string genreLabel,
        Genre genre,
        int year,
        int durationSeconds,
        string ageRating,
        long priceMinorUnits,
        string posterUrl,
        string trailerUrl,
        bool featured,
        string synopsis)
    {
        var movie = new Movie
        {
            Title = title,
            Slug = slug,
            Description = $"{title} — licensed demo catalog title.",
            Synopsis = synopsis,
            Language = "en",
            ReleaseYear = year,
            DurationSeconds = durationSeconds,
            AgeRating = ageRating,
            PosterUrl = posterUrl,
            BackdropUrl = posterUrl,
            TrailerUrl = trailerUrl,
            PriceMinorUnits = priceMinorUnits,
            Currency = "INR",
            PurchaseType = PurchaseType.Lifetime,
            SubscriptionEligible = true,
            PublicationStatus = PublicationStatus.Published,
            AvailabilityStart = DateTimeOffset.UtcNow.AddYears(-1),
            LicenseTerritories = ["IN"],
            IsFeatured = featured,
            MovieGenres = [new MovieGenre { Genre = genre }]
        };

        movie.License = new MovieLicense
        {
            Movie = movie,
            RightsHolder = "AKHRA Demo Licensor",
            LicenseReference = $"DEMO-{slug}",
            ValidFrom = DateTimeOffset.UtcNow.AddYears(-1),
            ValidTo = DateTimeOffset.UtcNow.AddYears(5),
            Territories = ["IN"],
            AllowsStreaming = true
        };

        return movie;
    }
}
