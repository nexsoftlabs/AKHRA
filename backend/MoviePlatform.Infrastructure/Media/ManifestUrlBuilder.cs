using Microsoft.Extensions.Options;

namespace MoviePlatform.Infrastructure.Media;

public static class ManifestUrlBuilder
{
    public static string ResolvePlaybackUrl(
        string storageKey,
        Guid movieId,
        string sessionToken,
        IOptions<MediaStorageOptions> mediaOptions,
        ICloudFrontUrlSigner cloudFront,
        DateTimeOffset expiresAt)
    {
        if (storageKey.StartsWith("http", StringComparison.OrdinalIgnoreCase))
        {
            if (cloudFront.IsConfigured)
            {
                var signed = cloudFront.SignManifestUrl(storageKey, expiresAt.UtcDateTime);
                if (!string.IsNullOrWhiteSpace(signed))
                {
                    return signed;
                }
            }

            return storageKey;
        }

        if (mediaOptions.Value.UseCloudFrontSigning)
        {
            var signed = cloudFront.SignManifestUrl(storageKey, expiresAt.UtcDateTime);
            if (!string.IsNullOrWhiteSpace(signed))
            {
                return signed;
            }
        }

        return $"/api/v1/playback/stream/{movieId}/manifest.m3u8?token={Uri.EscapeDataString(sessionToken)}";
    }
}
