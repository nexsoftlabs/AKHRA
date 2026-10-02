using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;

namespace MoviePlatform.Infrastructure.Media;

public interface IS3ObjectStorage
{
    bool IsConfigured { get; }
    Task<string> GetPresignedPutUrlAsync(string key, string contentType, TimeSpan ttl, CancellationToken ct);
    Task<Stream> OpenReadAsync(string key, CancellationToken ct);
    string GetS3Uri(string key);
}

public sealed class S3ObjectStorage(IOptions<MediaStorageOptions> options) : IS3ObjectStorage
{
    public bool IsConfigured => options.Value.UseS3;

    private IAmazonS3 Client => new AmazonS3Client(RegionEndpoint.GetBySystemName(options.Value.S3Region ?? "ap-south-1"));

    public Task<string> GetPresignedPutUrlAsync(string key, string contentType, TimeSpan ttl, CancellationToken ct)
    {
        var cfg = options.Value;
        var request = new GetPreSignedUrlRequest
        {
            BucketName = cfg.S3Bucket,
            Key = key,
            Verb = HttpVerb.PUT,
            ContentType = contentType,
            Expires = DateTime.UtcNow.Add(ttl),
        };
        return Task.FromResult(Client.GetPreSignedURL(request));
    }

    public async Task<Stream> OpenReadAsync(string key, CancellationToken ct)
    {
        var response = await Client.GetObjectAsync(options.Value.S3Bucket!, key, ct);
        return response.ResponseStream;
    }

    public string GetS3Uri(string key) => $"s3://{options.Value.S3Bucket}/{key}";
}
