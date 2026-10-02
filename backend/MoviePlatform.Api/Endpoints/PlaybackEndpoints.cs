using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MoviePlatform.Application.Media;
using MoviePlatform.Domain.Enums;
using MoviePlatform.Infrastructure.Media;
using MoviePlatform.Infrastructure.Persistence;

namespace MoviePlatform.Api.Endpoints;

public static class PlaybackEndpoints
{
    public static RouteGroupBuilder MapPlaybackEndpoints(this RouteGroupBuilder api)
    {
        var playback = api.MapGroup("/playback");

        playback.MapPost("/movies/{slug}/start", async (
            string slug,
            IPlaybackService playbackService,
            HttpContext ctx,
            CancellationToken ct) =>
        {
            var result = await playbackService.StartPlaybackAsync(
                slug,
                ctx.User,
                ctx.Connection.RemoteIpAddress?.ToString(),
                ct);

            return result is null
                ? Results.Json(
                    new { error = "playback_denied", message = "Purchase required or stream not ready." },
                    statusCode: StatusCodes.Status403Forbidden)
                : Results.Ok(result);
        }).RequireAuthorization();

        playback.MapGet("/library", async (IPlaybackService playbackService, HttpContext ctx, CancellationToken ct) =>
            Results.Ok(await playbackService.ListLibraryAsync(ctx.User, ct)))
            .RequireAuthorization();

        playback.MapGet("/manifest", async (
            string token,
            Guid movieId,
            IPlaybackService playbackService,
            MoviePlatformDbContext db,
            ICloudFrontUrlSigner cloudFront,
            IOptions<MediaStorageOptions> mediaOptions,
            CancellationToken ct) =>
        {
            if (!await playbackService.ValidateSessionTokenAsync(token, movieId, ct))
            {
                return Results.Unauthorized();
            }

            var asset = await db.MediaAssets
                .AsNoTracking()
                .Where(a => a.MovieId == movieId && a.AssetType == MediaAssetType.HlsManifest && a.IsReady)
                .OrderByDescending(a => a.CreatedAt)
                .FirstOrDefaultAsync(ct);

            if (asset is null)
            {
                return Results.NotFound();
            }

            var expires = DateTime.UtcNow.AddHours(4);
            var signed = ManifestUrlBuilder.ResolvePlaybackUrl(
                asset.StorageKey,
                movieId,
                token,
                mediaOptions,
                cloudFront,
                expires);

            if (signed.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                return Results.Redirect(signed);
            }

            var path = Path.Combine(Directory.GetCurrentDirectory(), mediaOptions.Value.LocalRoot, asset.StorageKey);
            return File.Exists(path) ? Results.File(path, "application/vnd.apple.mpegurl") : Results.NotFound();
        }).AllowAnonymous();

        return api;
    }
}
