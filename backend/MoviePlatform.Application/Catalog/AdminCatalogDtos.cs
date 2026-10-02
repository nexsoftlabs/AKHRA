using MoviePlatform.Domain.Enums;

namespace MoviePlatform.Application.Catalog;

public record AdminMovieListItemDto(
    Guid Id,
    string Title,
    string Slug,
    PublicationStatus PublicationStatus,
    long PriceMinorUnits,
    string Currency,
    DateTimeOffset UpdatedAt);

public record AdminMovieDetailDto(
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
    PurchaseType PurchaseType,
    bool SubscriptionEligible,
    PublicationStatus PublicationStatus,
    DateTimeOffset? AvailabilityStart,
    DateTimeOffset? AvailabilityEnd,
    string[] LicenseTerritories,
    bool IsFeatured,
    IReadOnlyList<string> Genres,
    MovieLicenseDto? License);

public record MovieLicenseDto(
    string RightsHolder,
    string LicenseReference,
    DateTimeOffset ValidFrom,
    DateTimeOffset ValidTo,
    string[] Territories,
    bool AllowsStreaming,
    string? Notes);

public record CreateMovieRequest(
    string Title,
    string? Slug,
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
    PurchaseType PurchaseType,
    bool SubscriptionEligible,
    bool IsFeatured,
    IReadOnlyList<string> Genres,
    MovieLicenseDto? License);

public record UpdateMovieRequest(
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
    PurchaseType PurchaseType,
    bool SubscriptionEligible,
    PublicationStatus PublicationStatus,
    DateTimeOffset? AvailabilityStart,
    DateTimeOffset? AvailabilityEnd,
    string[] LicenseTerritories,
    bool IsFeatured,
    IReadOnlyList<string> Genres,
    MovieLicenseDto? License);
