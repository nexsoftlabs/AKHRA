using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MoviePlatform.Application.Authorization;
using MoviePlatform.Application.Media;
using MoviePlatform.Domain.Enums;
using MoviePlatform.Infrastructure.Media;
using MoviePlatform.Infrastructure.Persistence;

namespace MoviePlatform.Api.Endpoints;

public static class AdminMediaEndpoints
{
    public static RouteGroupBuilder MapAdminMediaEndpoints(this RouteGroupBuilder api)
    {
        var admin = api.MapGroup("/admin/media")
            .RequireAuthorization(AppPolicies.ContentManagement)
            .RequireAuthorization("AdminMfa");

        admin.MapPost("/movies/{movieId:guid}/upload-sessions", async (
            Guid movieId,
            CreateUploadSessionRequest request,
            IMediaUploadService uploads,
            HttpContext ctx,
            CancellationToken ct) =>
        {
            try
            {
                var session = await uploads.CreateSourceUploadAsync(movieId, request, ctx.User, ct);
                return Results.Ok(session);
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Unauthorized();
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }
        });

        admin.MapPut("/upload/{sessionId:guid}", async (
            Guid sessionId,
            HttpRequest request,
            IMediaUploadService uploads,
            MoviePlatformDbContext db,
            IOptions<MediaStorageOptions> options,
            HttpContext ctx,
            CancellationToken ct) =>
        {
            var session = await db.UploadSessions.FirstOrDefaultAsync(s => s.Id == sessionId, ct);
            if (session is null || session.Status != UploadSessionStatus.Pending || session.ExpiresAt < DateTimeOffset.UtcNow)
            {
                return Results.NotFound();
            }

            if (!request.HasFormContentType)
            {
                return Results.BadRequest(new { message = "Multipart form upload required." });
            }

            var form = await request.ReadFormAsync(ct);
            var file = form.Files.FirstOrDefault();
            if (file is null || file.Length == 0)
            {
                return Results.BadRequest(new { message = "File is required." });
            }

            if (session.MaxSizeBytes is not null && file.Length > session.MaxSizeBytes)
            {
                return Results.BadRequest(new { message = "File exceeds maximum size." });
            }

            var root = Path.Combine(Directory.GetCurrentDirectory(), options.Value.LocalRoot);
            Directory.CreateDirectory(root);
            var path = Path.Combine(root, session.StorageKey.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);

            await using (var stream = File.Create(path))
            {
                await file.CopyToAsync(stream, ct);
            }

            await db.SaveChangesAsync(ct);

            var queued = await uploads.CompleteUploadAsync(sessionId, ctx.User, ct);
            return queued
                ? Results.Ok(new { status = "uploaded", bytes = file.Length, encoding = "queued" })
                : Results.BadRequest(new { message = "Upload saved but encoding could not be queued." });
        }).DisableAntiforgery();

        admin.MapPost("/upload-sessions/{sessionId:guid}/complete", async (
            Guid sessionId,
            IMediaUploadService uploads,
            HttpContext ctx,
            CancellationToken ct) =>
        {
            var ok = await uploads.CompleteUploadAsync(sessionId, ctx.User, ct);
            return ok ? Results.Ok(new { status = "encoding_queued" }) : Results.NotFound();
        });

        return api;
    }
}
