using System.Security.Claims;

namespace MoviePlatform.Application.Catalog;

public interface IAdminCatalogService
{
    Task<IReadOnlyList<AdminMovieListItemDto>> ListAsync(
        string? search,
        string? status,
        CancellationToken cancellationToken);

    Task<AdminMovieDetailDto?> GetAsync(Guid id, CancellationToken cancellationToken);

    Task<AdminMovieDetailDto> CreateAsync(CreateMovieRequest request, ClaimsPrincipal user, CancellationToken cancellationToken);

    Task<AdminMovieDetailDto?> UpdateAsync(Guid id, UpdateMovieRequest request, CancellationToken cancellationToken);

    Task<bool> PublishAsync(Guid id, CancellationToken cancellationToken);

    Task<bool> UnpublishAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<string>> ListGenreSlugsAsync(CancellationToken cancellationToken);
}
