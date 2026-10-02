using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MoviePlatform.Application.Media;
using MoviePlatform.Domain.Enums;
using MoviePlatform.Domain.Media;
using MoviePlatform.Infrastructure.Persistence;

namespace MoviePlatform.Infrastructure.Media;

public sealed class MediaUploadService(
    MoviePlatformDbContext db,
    IOptions<MediaStorageOptions> options,
    IS3ObjectStorage s3) : IMediaUploadService
{
    public async Task<UploadSessionDto> CreateSourceUploadAsync(
        Guid movieId,
        CreateUploadSessionRequest request,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(user);
        if (userId is null)
        {
            throw new UnauthorizedAccessException();
        }

        var movie = await db.Movies.FirstOrDefaultAsync(m => m.Id == movieId, cancellationToken);
        if (movie is null)
        {
            throw new InvalidOperationException("Movie not found.");
        }

        var storageKey = $"movies/{movieId:N}/source/{Guid.CreateVersion7():N}";
        var session = new UploadSession
        {
            MovieId = movieId,
            RequestedByUserId = userId.Value,
            StorageKey = storageKey,
            Status = UploadSessionStatus.Pending,
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(2),
            MaxSizeBytes = request.SizeBytes ?? 10L * 1024 * 1024 * 1024,
            ContentType = request.ContentType,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        db.UploadSessions.Add(session);
        await db.SaveChangesAsync(cancellationToken);

        var uploadUrl = s3.IsConfigured
            ? await s3.GetPresignedPutUrlAsync(
                storageKey,
                request.ContentType ?? "video/mp4",
                TimeSpan.FromHours(2),
                cancellationToken)
            : $"/api/v1/admin/media/upload/{session.Id}";

        return new UploadSessionDto(session.Id, movieId, uploadUrl, storageKey, session.ExpiresAt, session.MaxSizeBytes);
    }

    public async Task<bool> CompleteUploadAsync(Guid sessionId, ClaimsPrincipal user, CancellationToken cancellationToken)
    {
        var userId = GetUserId(user);
        if (userId is null)
        {
            return false;
        }

        var session = await db.UploadSessions
            .Include(s => s.Movie)
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.RequestedByUserId == userId, cancellationToken);

        if (session is null || session.Status != UploadSessionStatus.Pending)
        {
            return false;
        }

        session.Status = UploadSessionStatus.Completed;
        session.CompletedAt = DateTimeOffset.UtcNow;

        var sourceAsset = new MediaAsset
        {
            MovieId = session.MovieId,
            AssetType = MediaAssetType.SourceVideo,
            StorageKey = session.StorageKey,
            ContentType = session.ContentType,
            IsReady = true,
        };
        db.MediaAssets.Add(sourceAsset);

        db.EncodingJobs.Add(new EncodingJob
        {
            MovieId = session.MovieId,
            SourceAssetId = sourceAsset.Id,
            Status = EncodingJobStatus.Queued,
        });

        session.Movie.PublicationStatus = PublicationStatus.Processing;
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static Guid? GetUserId(ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var id) ? id : null;
    }
}
