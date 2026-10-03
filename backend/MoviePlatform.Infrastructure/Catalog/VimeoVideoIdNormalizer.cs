using System.Text.RegularExpressions;

namespace MoviePlatform.Infrastructure.Catalog;

internal static partial class VimeoVideoIdNormalizer
{
    public static string? Normalize(string? urlOrId)
    {
        if (string.IsNullOrWhiteSpace(urlOrId))
        {
            return null;
        }

        var trimmed = urlOrId.Trim();
        if (NumericId().IsMatch(trimmed))
        {
            return trimmed;
        }

        if (!Uri.TryCreate(trimmed, UriKind.Absolute, out var uri) ||
            !uri.Host.Contains("vimeo.com", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        foreach (var segment in uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries))
        {
            if (NumericId().IsMatch(segment))
            {
                return segment;
            }
        }

        return null;
    }

    [GeneratedRegex(@"^\d+$")]
    private static partial Regex NumericId();
}
