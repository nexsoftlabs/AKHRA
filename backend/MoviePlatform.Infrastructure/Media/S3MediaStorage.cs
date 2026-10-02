using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;

namespace MoviePlatform.Infrastructure.Media;

public interface IMediaBlobStorage
{
    Task<string> GetUploadUrlAsync(string storageKey, string contentType, TimeSpan ttl, CancellationToken cancellationToken);
}

public sealed class S3MediaStorage(IOptions<MediaStorageOptions> options) : IMediaBlobStorage
{
    public async Task<string> GetUploadUrlAsync(
        string storageKey,
        string contentType,
        TimeSpan ttl,
        CancellationToken cancellationToken)
    {
        var cfg = options.Value;
        if (!cfg.UseS3 || string.IsNullOrWhiteSpace(cfg.S3Bucket))
        {
            throw new InvalidOperationException("S3 is not configured.");
        }

        var region = RegionEndpoint.GetBySystemName(cfg.S3Region ?? "ap-south-1");
        using var client = new AmazonS3Client(region);
        var request = new GetPreSignedUrlRequest
        {
            BucketName = cfg.S3Bucket,
            Key = storageKey,
            Verb = HttpVerb.PUT,
            ContentType = contentType,
            Expires = DateTime.UtcNow.Add(ttl),
        };

        return await Task.FromResult(client.GetPreSignedURL(request));
    }
}

public sealed class LocalMediaBlobStorage : IMediaBlobStorage
{
    public Task<string> GetUploadUrlAsync(
        string storageKey,
        string contentType,
        TimeSpan ttl,
        CancellationToken cancellationToken) =>
        Task.FromResult($"/api/v1/admin/media/upload-local?key={Uri.EscapeDataString(storageKey)}");
}
