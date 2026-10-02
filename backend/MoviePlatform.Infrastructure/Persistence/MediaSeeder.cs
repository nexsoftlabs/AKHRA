using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MoviePlatform.Domain.Enums;
using MoviePlatform.Domain.Media;
using MoviePlatform.Infrastructure.Media;

namespace MoviePlatform.Infrastructure.Persistence;

public static class MediaSeeder
{
    public static async Task SeedAsync(MoviePlatformDbContext db, MediaStorageOptions options, CancellationToken cancellationToken = default)
    {
        var moviesWithoutHls = await db.Movies
            .Include(m => m.MediaAssets)
            .Where(m => !m.MediaAssets.Any(a => a.AssetType == MediaAssetType.HlsManifest && a.IsReady))
            .ToListAsync(cancellationToken);

        foreach (var movie in moviesWithoutHls)
        {
            db.MediaAssets.Add(new MediaAsset
            {
                MovieId = movie.Id,
                AssetType = MediaAssetType.HlsManifest,
                StorageKey = options.DevHlsManifestUrl,
                ContentType = "application/vnd.apple.mpegurl",
                IsReady = true,
            });
        }

        if (moviesWithoutHls.Count > 0)
        {
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
