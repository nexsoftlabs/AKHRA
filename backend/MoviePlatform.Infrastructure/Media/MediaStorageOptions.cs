namespace MoviePlatform.Infrastructure.Media;

public sealed class MediaStorageOptions
{
    public const string SectionName = "Media";

    /// <summary>Local directory for dev uploads (relative to content root).</summary>
    public string LocalRoot { get; set; } = "media-uploads";

    public string? S3Bucket { get; set; }
    public string? S3Region { get; set; }
    public string? CloudFrontDomain { get; set; }
    public string? CloudFrontKeyPairId { get; set; }

    /// <summary>RSA private key PEM for signed URLs (store in secret manager in production).</summary>
    public string? CloudFrontPrivateKeyPem { get; set; }

    public bool UseS3 => !string.IsNullOrWhiteSpace(S3Bucket);

    public bool UseCloudFrontSigning =>
        !string.IsNullOrWhiteSpace(CloudFrontDomain) &&
        !string.IsNullOrWhiteSpace(CloudFrontKeyPairId) &&
        !string.IsNullOrWhiteSpace(CloudFrontPrivateKeyPem);

    public string? MediaConvertRoleArn { get; set; }
    public string? MediaConvertQueueArn { get; set; }
    public string? MediaConvertEndpoint { get; set; }

    public bool UseMediaConvert =>
        UseS3 &&
        !string.IsNullOrWhiteSpace(MediaConvertRoleArn) &&
        !string.IsNullOrWhiteSpace(MediaConvertQueueArn);

    /// <summary>Demo HLS used when encoding completes in dev.</summary>
    public string DevHlsManifestUrl { get; set; } = "https://test-streams.mux.dev/x36xhzz/x36xhzz.m3u8";
}
