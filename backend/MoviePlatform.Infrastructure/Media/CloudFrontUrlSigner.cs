using System.IO;
using Amazon.CloudFront;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MoviePlatform.Infrastructure.Media;

public interface ICloudFrontUrlSigner
{
    bool IsConfigured { get; }

    string? SignManifestUrl(string resourcePathOrUrl, DateTime expiresUtc);
}

public sealed class CloudFrontUrlSigner(
    IOptions<MediaStorageOptions> options,
    ILogger<CloudFrontUrlSigner> logger) : ICloudFrontUrlSigner
{
    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(options.Value.CloudFrontDomain) &&
        !string.IsNullOrWhiteSpace(options.Value.CloudFrontKeyPairId) &&
        !string.IsNullOrWhiteSpace(options.Value.CloudFrontPrivateKeyPem);

    public string? SignManifestUrl(string resourcePathOrUrl, DateTime expiresUtc)
    {
        if (!IsConfigured)
        {
            return null;
        }

        var cfg = options.Value;
        var resourcePath = resourcePathOrUrl;

        if (resourcePath.StartsWith("http", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                var uri = new Uri(resourcePath);
                resourcePath = string.IsNullOrEmpty(uri.PathAndQuery) ? "/" : uri.PathAndQuery;
            }
            catch
            {
                return null;
            }
        }

        if (!resourcePath.StartsWith('/'))
        {
            resourcePath = "/" + resourcePath;
        }

        try
        {
            using var keyReader = new StringReader(cfg.CloudFrontPrivateKeyPem!);
            return AmazonCloudFrontUrlSigner.GetCannedSignedURL(
                AmazonCloudFrontUrlSigner.Protocol.https,
                cfg.CloudFrontDomain!,
                keyReader,
                resourcePath,
                cfg.CloudFrontKeyPairId!,
                expiresUtc);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "CloudFront signing failed for {Path}", resourcePath);
            return null;
        }
    }
}
