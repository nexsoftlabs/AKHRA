using System.Security.Claims;

namespace MoviePlatform.Application.Catalog;

public interface ICatalogService
{
    Task<IReadOnlyList<MovieListItemDto>> ListPublishedAsync(string? search, string? genreSlug, CancellationToken cancellationToken);
    Task<MovieDetailDto?> GetPublishedBySlugAsync(string slug, ClaimsPrincipal? user, CancellationToken cancellationToken);

    Task<IReadOnlyList<string>> ListPublishedGenreSlugsAsync(CancellationToken cancellationToken);
}
