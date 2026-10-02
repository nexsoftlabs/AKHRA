using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MoviePlatform.Domain.Enums;
using MoviePlatform.Domain.Media;
using MoviePlatform.Infrastructure.Media;
using MoviePlatform.Infrastructure.Persistence;

namespace MoviePlatform.Worker;

public sealed class EncodingJobWorker(
    IServiceScopeFactory scopeFactory,
    IOptions<MediaStorageOptions> mediaOptions,
    ILogger<EncodingJobWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Encoding job worker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessQueuedAsync(stoppingToken);
                await ProcessInProgressAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Encoding batch failed.");
            }

            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }

    private async Task ProcessQueuedAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<MoviePlatformDbContext>();
        var s3 = scope.ServiceProvider.GetRequiredService<IS3ObjectStorage>();
        var mediaConvert = scope.ServiceProvider.GetRequiredService<IMediaConvertEncodingService>();

        var jobs = await db.EncodingJobs
            .Include(j => j.SourceAsset)
            .Where(j => j.Status == EncodingJobStatus.Queued)
            .OrderBy(j => j.CreatedAt)
            .Take(3)
            .ToListAsync(cancellationToken);

        foreach (var job in jobs)
        {
            job.Status = EncodingJobStatus.InProgress;
            await db.SaveChangesAsync(cancellationToken);

            try
            {
                if (mediaOptions.Value.UseMediaConvert && job.SourceAsset is not null && s3.IsConfigured)
                {
                    var outputPrefix = $"hls/{job.MovieId:N}";
                    var sourceUri = s3.GetS3Uri(job.SourceAsset.StorageKey);
                    job.ExternalJobId = await mediaConvert.SubmitHlsJobAsync(sourceUri, outputPrefix, cancellationToken);
                    await db.SaveChangesAsync(cancellationToken);
                    continue;
                }

                await CompleteWithDevManifestAsync(db, job, mediaOptions.Value, cancellationToken);
            }
            catch (Exception ex)
            {
                job.Status = EncodingJobStatus.Failed;
                job.ErrorMessage = ex.Message;
                job.RetryCount++;
                await db.SaveChangesAsync(cancellationToken);
            }
        }
    }

    private async Task ProcessInProgressAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<MoviePlatformDbContext>();
        var mediaConvert = scope.ServiceProvider.GetRequiredService<IMediaConvertEncodingService>();

        if (!mediaOptions.Value.UseMediaConvert)
        {
            return;
        }

        var jobs = await db.EncodingJobs
            .Where(j => j.Status == EncodingJobStatus.InProgress && j.ExternalJobId != null)
            .Take(5)
            .ToListAsync(cancellationToken);

        foreach (var job in jobs)
        {
            var status = await mediaConvert.GetJobStatusAsync(job.ExternalJobId!, cancellationToken);
            if (status is null)
            {
                continue;
            }

            if (status is "COMPLETE")
            {
                var outputPrefix = $"hls/{job.MovieId:N}";
                var manifestKey = mediaConvert.BuildManifestKey(outputPrefix);
                db.MediaAssets.Add(new MediaAsset
                {
                    MovieId = job.MovieId,
                    AssetType = MediaAssetType.HlsManifest,
                    StorageKey = manifestKey,
                    ContentType = "application/vnd.apple.mpegurl",
                    IsReady = true,
                });
                await MarkMovieReadyAsync(db, job.MovieId, cancellationToken);
                job.Status = EncodingJobStatus.Succeeded;
            }
            else if (status is "ERROR" or "CANCELED")
            {
                job.Status = EncodingJobStatus.Failed;
                job.ErrorMessage = $"MediaConvert status: {status}";
            }

            await db.SaveChangesAsync(cancellationToken);
        }
    }

    private static async Task CompleteWithDevManifestAsync(
        MoviePlatformDbContext db,
        EncodingJob job,
        MediaStorageOptions options,
        CancellationToken cancellationToken)
    {
        var manifestUrl = options.DevHlsManifestUrl;

        db.MediaAssets.Add(new MediaAsset
        {
            MovieId = job.MovieId,
            AssetType = MediaAssetType.HlsManifest,
            StorageKey = manifestUrl,
            ContentType = "application/vnd.apple.mpegurl",
            IsReady = true,
        });

        await MarkMovieReadyAsync(db, job.MovieId, cancellationToken);
        job.Status = EncodingJobStatus.Succeeded;
        job.ErrorMessage = null;
        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task MarkMovieReadyAsync(MoviePlatformDbContext db, Guid movieId, CancellationToken cancellationToken)
    {
        var movie = await db.Movies.FindAsync([movieId], cancellationToken);
        if (movie is not null && movie.PublicationStatus == PublicationStatus.Processing)
        {
            movie.PublicationStatus = PublicationStatus.Ready;
        }
    }
}
