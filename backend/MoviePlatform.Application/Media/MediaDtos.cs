namespace MoviePlatform.Application.Media;

public record CreateUploadSessionRequest(string ContentType, long? SizeBytes);

public record UploadSessionDto(
    Guid SessionId,
    Guid MovieId,
    string UploadUrl,
    string StorageKey,
    DateTimeOffset ExpiresAt,
    long? MaxSizeBytes);

public record PlaybackStartDto(
    string SessionToken,
    string ManifestUrl,
    DateTimeOffset ExpiresAt);

public record LibraryItemDto(
    Guid MovieId,
    string Title,
    string Slug,
    string? PosterUrl,
    int DurationSeconds,
    DateTimeOffset? PurchasedAt);
