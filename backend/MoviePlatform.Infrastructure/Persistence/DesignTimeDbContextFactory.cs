using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MoviePlatform.Infrastructure.Persistence;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<MoviePlatformDbContext>
{
    public MoviePlatformDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<MoviePlatformDbContext>();
        var connectionString = Environment.GetEnvironmentVariable("MOVIEPLATFORM_CONNECTION")
            ?? "Host=localhost;Port=5432;Database=movieplatform;Username=movieplatform;Password=movieplatform_dev";

        optionsBuilder.UseNpgsql(connectionString);
        return new MoviePlatformDbContext(optionsBuilder.Options);
    }
}
