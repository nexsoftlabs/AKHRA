using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MoviePlatform.Domain.Enums;
using MoviePlatform.Infrastructure.Persistence;

namespace MoviePlatform.Infrastructure.Media;

public interface IHlsStreamService
{
    Task<Stream?> OpenManifestAsync(Guid movieId, string sessionToken, CancellationToken ct);
    Task<Stream?> OpenSegmentAsync(Guid movieId, string sessionToken, string segmentPath, CancellationToken ct);
}

public partial class HlsStreamService(
    MoviePlatformDbContext db,
    IS3ObjectStorage s3,
    IOptions<MediaStorageOptions> mediaOptions) : IHlsStreamService
{
    public async Task<Stream?> OpenManifestAsync(Guid movieId, string sessionToken, CancellationToken ct)
    {
        if (!await PlaybackService.ValidateSessionStatic(db, movieId, sessionToken, ct))
        {
            return null;
        }

        var asset = await GetManifestAssetAsync(movieId, ct);
        if (asset is null)
        {
            return null;
        }

        var raw = await ReadStorageAsync(asset.StorageKey, ct);
        if (raw is null)
        {
            return null;
        }

        using var reader = new StreamReader(raw, leaveOpen: false);
        var text = await reader.ReadToEndAsync(ct);
        var proxyBase = $"/api/v1/playback/stream/{movieId}";
        var rewritten = RewriteManifestLines(text, proxyBase, sessionToken);
        return new MemoryStream(Encoding.UTF8.GetBytes(rewritten));
    }

    public async Task<Stream?> OpenSegmentAsync(Guid movieId, string sessionToken, string segmentPath, CancellationToken ct)
    {
        if (!await PlaybackService.ValidateSessionStatic(db, movieId, sessionToken, ct))
        {
            return null;
        }

        var asset = await GetManifestAssetAsync(movieId, ct);
        if (asset is null)
        {
            return null;
        }

        var basePrefix = GetHlsPrefix(asset.StorageKey);
        var key = string.IsNullOrEmpty(basePrefix)
            ? segmentPath.TrimStart('/')
            : $"{basePrefix}/{segmentPath.TrimStart('/')}";

        return await ReadStorageAsync(key, ct);
    }

    private async Task<Domain.Media.MediaAsset?> GetManifestAssetAsync(Guid movieId, CancellationToken ct) =>
        await db.MediaAssets
            .AsNoTracking()
            .Where(a => a.MovieId == movieId && a.AssetType == MediaAssetType.HlsManifest && a.IsReady)
            .OrderByDescending(a => a.CreatedAt)
            .FirstOrDefaultAsync(ct);

    private async Task<Stream?> ReadStorageAsync(string storageKey, CancellationToken ct)
    {
        if (storageKey.StartsWith("http", StringComparison.OrdinalIgnoreCase))
        {
            using var http = new HttpClient();
            var bytes = await http.GetByteArrayAsync(storageKey, ct);
            return new MemoryStream(bytes);
        }

        if (s3.IsConfigured && !storageKey.Contains(".."))
        {
            return await s3.OpenReadAsync(storageKey, ct);
        }

        var path = Path.Combine(Directory.GetCurrentDirectory(), mediaOptions.Value.LocalRoot, storageKey);
        return File.Exists(path) ? File.OpenRead(path) : null;
    }

    private static string GetHlsPrefix(string manifestKey)
    {
        if (manifestKey.StartsWith("http", StringComparison.OrdinalIgnoreCase))
        {
            return string.Empty;
        }

        var idx = manifestKey.LastIndexOf('/');
        return idx > 0 ? manifestKey[..idx] : string.Empty;
    }

    private static string RewriteManifestLines(string manifest, string proxyBase, string token)
    {
        var sb = new StringBuilder();
        foreach (var line in manifest.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            var trimmed = line.Trim();
            if (trimmed.Length == 0 || trimmed.StartsWith('#'))
            {
                sb.AppendLine(trimmed);
                continue;
            }

            if (!trimmed.Contains('/') && !trimmed.EndsWith(".ts", StringComparison.OrdinalIgnoreCase) &&
                !trimmed.EndsWith(".m4s", StringComparison.OrdinalIgnoreCase))
            {
                sb.AppendLine(trimmed);
                continue;
            }

            sb.AppendLine($"{proxyBase}/segment?token={Uri.EscapeDataString(token)}&path={Uri.EscapeDataString(trimmed)}");
        }

        return sb.ToString();
    }
}
