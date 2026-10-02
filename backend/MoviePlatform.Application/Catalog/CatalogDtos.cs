namespace MoviePlatform.Application.Catalog;

public record MovieListItemDto(
    Guid Id,
    string Title,
    string Slug,
    string? PosterUrl,
    int? ReleaseYear,
    int DurationSeconds,
    string? AgeRating,
    long PriceMinorUnits,
    string Currency,
    bool IsFeatured,
    IReadOnlyList<string> Genres);

public record MovieDetailDto(
    Guid Id,
    string Title,
    string Slug,
    string? Description,
    string? Synopsis,
    string? Language,
    int? ReleaseYear,
    int DurationSeconds,
    string? AgeRating,
    string? PosterUrl,
    string? BackdropUrl,
    string? TrailerUrl,
    long PriceMinorUnits,
    string Currency,
    string PurchaseType,
    bool SubscriptionEligible,
    bool IsFeatured,
    IReadOnlyList<string> Genres,
    bool HasAccess);
