using MoviePlatform.Domain.Common;

namespace MoviePlatform.Domain.Catalog;

public class Genre : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public ICollection<MovieGenre> MovieGenres { get; set; } = new List<MovieGenre>();
}
