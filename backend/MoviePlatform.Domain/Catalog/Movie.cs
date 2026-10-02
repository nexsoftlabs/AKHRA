using MoviePlatform.Domain.Common;
using MoviePlatform.Domain.Enums;
using MoviePlatform.Domain.Media;

namespace MoviePlatform.Domain.Catalog;

public class Movie : AuditableEntity
{
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Synopsis { get; set; }
    public string? Language { get; set; }
    public int? ReleaseYear { get; set; }
    public int DurationSeconds { get; set; }
    public string? AgeRating { get; set; }
    public string? PosterUrl { get; set; }
    public string? BackdropUrl { get; set; }
    public string? TrailerUrl { get; set; }
    public long PriceMinorUnits { get; set; }
    public string Currency { get; set; } = "INR";
    public PurchaseType PurchaseType { get; set; } = PurchaseType.Lifetime;
    public int? RentalDurationHours { get; set; }
    public bool SubscriptionEligible { get; set; }
    public PublicationStatus PublicationStatus { get; set; } = PublicationStatus.Draft;
    public DateTimeOffset? AvailabilityStart { get; set; }
    public DateTimeOffset? AvailabilityEnd { get; set; }
    public string[] LicenseTerritories { get; set; } = ["IN"];
    public bool IsFeatured { get; set; }
    public Guid? CreatedByUserId { get; set; }

    public ICollection<MovieGenre> MovieGenres { get; set; } = new List<MovieGenre>();
    public ICollection<MediaAsset> MediaAssets { get; set; } = new List<MediaAsset>();
    public MovieLicense? License { get; set; }
}

public class MovieGenre
{
    public Guid MovieId { get; set; }
    public Movie Movie { get; set; } = null!;
    public Guid GenreId { get; set; }
    public Genre Genre { get; set; } = null!;
}

public class MovieLicense : AuditableEntity
{
    public Guid MovieId { get; set; }
    public Movie Movie { get; set; } = null!;
    public string RightsHolder { get; set; } = string.Empty;
    public string LicenseReference { get; set; } = string.Empty;
    public DateTimeOffset ValidFrom { get; set; }
    public DateTimeOffset ValidTo { get; set; }
    public string[] Territories { get; set; } = ["IN"];
    public bool AllowsStreaming { get; set; } = true;
    public string? Notes { get; set; }
}
