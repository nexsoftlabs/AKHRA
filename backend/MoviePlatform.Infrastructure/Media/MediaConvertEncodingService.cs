using Amazon.MediaConvert;
using Amazon.MediaConvert.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MoviePlatform.Infrastructure.Media;

public interface IMediaConvertEncodingService
{
    bool IsConfigured { get; }
    Task<string> SubmitHlsJobAsync(string sourceS3Uri, string outputS3Prefix, CancellationToken ct);
    Task<string?> GetJobStatusAsync(string jobId, CancellationToken ct);
    string BuildManifestKey(string outputPrefix) => $"{outputPrefix.TrimEnd('/')}/master.m3u8";
}

public sealed class MediaConvertEncodingService(
    IOptions<MediaStorageOptions> options,
    ILogger<MediaConvertEncodingService> logger) : IMediaConvertEncodingService
{
    public bool IsConfigured => options.Value.UseMediaConvert;

    public async Task<string> SubmitHlsJobAsync(string sourceS3Uri, string outputS3Prefix, CancellationToken ct)
    {
        var cfg = options.Value;
        var clientConfig = new AmazonMediaConvertConfig
        {
            ServiceURL = cfg.MediaConvertEndpoint,
        };
        using var client = new AmazonMediaConvertClient(clientConfig);

        var outputPath = $"s3://{cfg.S3Bucket}/{outputS3Prefix.TrimEnd('/')}/";
        var request = new CreateJobRequest
        {
            Role = cfg.MediaConvertRoleArn,
            Queue = cfg.MediaConvertQueueArn,
            Settings = new JobSettings
            {
                Inputs =
                [
                    new Input { FileInput = sourceS3Uri },
                ],
                OutputGroups =
                [
                    new OutputGroup
                    {
                        Name = "HLS",
                        OutputGroupSettings = new OutputGroupSettings
                        {
                            Type = OutputGroupType.HLS_GROUP_SETTINGS,
                            HlsGroupSettings = new HlsGroupSettings
                            {
                                Destination = outputPath,
                                SegmentLength = 6,
                            },
                        },
                        Outputs =
                        [
                            new Output
                            {
                                ContainerSettings = new ContainerSettings
                                {
                                    Container = ContainerType.M3U8,
                                },
                                VideoDescription = new VideoDescription
                                {
                                    CodecSettings = new VideoCodecSettings
                                    {
                                        Codec = VideoCodec.H_264,
                                        H264Settings = new H264Settings
                                        {
                                            RateControlMode = H264RateControlMode.QVBR,
                                            MaxBitrate = 3_500_000,
                                        },
                                    },
                                },
                                AudioDescriptions =
                                [
                                    new AudioDescription
                                    {
                                        CodecSettings = new AudioCodecSettings
                                        {
                                            Codec = AudioCodec.AAC,
                                            AacSettings = new AacSettings
                                            {
                                                Bitrate = 96000,
                                                CodingMode = AacCodingMode.CODING_MODE_2_0,
                                                SampleRate = 48000,
                                            },
                                        },
                                    },
                                ],
                            },
                        ],
                    },
                ],
            },
        };

        var response = await client.CreateJobAsync(request, ct);
        var jobId = response.Job?.Id ?? throw new InvalidOperationException("MediaConvert returned no job id.");
        logger.LogInformation("MediaConvert job {JobId} submitted for {Source}", jobId, sourceS3Uri);
        return jobId;
    }

    public async Task<string?> GetJobStatusAsync(string jobId, CancellationToken ct)
    {
        var cfg = options.Value;
        var clientConfig = new AmazonMediaConvertConfig { ServiceURL = cfg.MediaConvertEndpoint };
        using var client = new AmazonMediaConvertClient(clientConfig);
        var response = await client.GetJobAsync(new GetJobRequest { Id = jobId }, ct);
        return response.Job?.Status?.Value;
    }
}
