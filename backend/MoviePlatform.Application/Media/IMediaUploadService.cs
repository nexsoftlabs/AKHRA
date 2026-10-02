using System.Security.Claims;

namespace MoviePlatform.Application.Media;

public interface IMediaUploadService
{
    Task<UploadSessionDto> CreateSourceUploadAsync(Guid movieId, CreateUploadSessionRequest request, ClaimsPrincipal user, CancellationToken cancellationToken);

    Task<bool> CompleteUploadAsync(Guid sessionId, ClaimsPrincipal user, CancellationToken cancellationToken);
}
