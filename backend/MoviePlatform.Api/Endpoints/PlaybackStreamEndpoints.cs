using MoviePlatform.Infrastructure.Media;

namespace MoviePlatform.Api.Endpoints;

public static class PlaybackStreamEndpoints
{
    public static RouteGroupBuilder MapPlaybackStreamEndpoints(this RouteGroupBuilder api)
    {
        var stream = api.MapGroup("/playback/stream");

        stream.MapGet("/{movieId:guid}/manifest.m3u8", async (
            Guid movieId,
            string token,
            IHlsStreamService hls,
            CancellationToken ct) =>
        {
            var body = await hls.OpenManifestAsync(movieId, token, ct);
            return body is null ? Results.Unauthorized() : Results.File(body, "application/vnd.apple.mpegurl");
        }).AllowAnonymous();

        stream.MapGet("/{movieId:guid}/segment", async (
            Guid movieId,
            string token,
            string path,
            IHlsStreamService hls,
            CancellationToken ct) =>
        {
            var body = await hls.OpenSegmentAsync(movieId, token, path, ct);
            if (body is null)
            {
                return Results.Unauthorized();
            }

            var contentType = path.EndsWith(".m3u8", StringComparison.OrdinalIgnoreCase)
                ? "application/vnd.apple.mpegurl"
                : "video/mp2t";
            return Results.File(body, contentType);
        }).AllowAnonymous();

        return api;
    }
}
