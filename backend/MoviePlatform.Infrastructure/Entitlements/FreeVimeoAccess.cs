using MoviePlatform.Domain.Catalog;

namespace MoviePlatform.Infrastructure.Entitlements;

internal static class FreeVimeoAccess
{
    public static bool IsFreeVimeoTitle(Movie movie) =>
        movie.PriceMinorUnits == 0 && !string.IsNullOrWhiteSpace(movie.VimeoVideoId);
}
