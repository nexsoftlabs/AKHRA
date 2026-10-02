using MoviePlatform.Domain.Catalog;
using MoviePlatform.Domain.Common;
using MoviePlatform.Domain.Enums;
using MoviePlatform.Domain.Identity;

namespace MoviePlatform.Domain.Media;

public class MediaAsset : AuditableEntity
{
    public Guid MovieId { get; set; }
    public Movie Movie { get; set; } = null!;
    public MediaAssetType AssetType { get; set; }
    public string StorageKey { get; set; } = string.Empty;
    public string? ContentType { get; set; }
    public long? SizeBytes { get; set; }
    public string? Checksum { get; set; }
    public bool IsReady { get; set; }
}

public class UploadSession : Entity
{
    public Guid MovieId { get; set; }
    public Movie Movie { get; set; } = null!;
    public Guid RequestedByUserId { get; set; }
    public ApplicationUser RequestedByUser { get; set; } = null!;
    public string StorageKey { get; set; } = string.Empty;
    public UploadSessionStatus Status { get; set; } = UploadSessionStatus.Pending;
    public DateTimeOffset ExpiresAt { get; set; }
    public long? MaxSizeBytes { get; set; }
    public string? ContentType { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
}

public class EncodingJob : AuditableEntity
{
    public Guid MovieId { get; set; }
    public Movie Movie { get; set; } = null!;
    public Guid? SourceAssetId { get; set; }
    public MediaAsset? SourceAsset { get; set; }
    public EncodingJobStatus Status { get; set; } = EncodingJobStatus.Queued;
    public string? ExternalJobId { get; set; }
    public string? ErrorMessage { get; set; }
    public int RetryCount { get; set; }
}
