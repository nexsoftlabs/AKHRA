using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MoviePlatform.Domain.Catalog;

namespace MoviePlatform.Infrastructure.Persistence.Configurations;

public class MovieConfiguration : IEntityTypeConfiguration<Movie>
{
    public void Configure(EntityTypeBuilder<Movie> builder)
    {
        builder.ToTable("movies");
        builder.HasIndex(m => m.Slug).IsUnique();
        builder.HasIndex(m => m.PublicationStatus);
        builder.Property(m => m.Title).HasMaxLength(500).IsRequired();
        builder.Property(m => m.Slug).HasMaxLength(500).IsRequired();
        builder.Property(m => m.Currency).HasMaxLength(3).IsRequired();
        builder.Property(m => m.LicenseTerritories).HasColumnType("text[]");
        builder.HasOne(m => m.License).WithOne(l => l.Movie).HasForeignKey<MovieLicense>(l => l.MovieId);
    }
}

public class GenreConfiguration : IEntityTypeConfiguration<Genre>
{
    public void Configure(EntityTypeBuilder<Genre> builder)
    {
        builder.ToTable("genres");
        builder.HasIndex(g => g.Slug).IsUnique();
        builder.Property(g => g.Name).HasMaxLength(200).IsRequired();
    }
}

public class MovieGenreConfiguration : IEntityTypeConfiguration<MovieGenre>
{
    public void Configure(EntityTypeBuilder<MovieGenre> builder)
    {
        builder.ToTable("movie_genres");
        builder.HasKey(mg => new { mg.MovieId, mg.GenreId });
    }
}

public class MovieLicenseConfiguration : IEntityTypeConfiguration<MovieLicense>
{
    public void Configure(EntityTypeBuilder<MovieLicense> builder)
    {
        builder.ToTable("movie_licenses");
        builder.Property(l => l.Territories).HasColumnType("text[]");
        builder.HasIndex(l => l.MovieId).IsUnique();
    }
}
