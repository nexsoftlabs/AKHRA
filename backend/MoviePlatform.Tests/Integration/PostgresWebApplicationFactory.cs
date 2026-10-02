using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace MoviePlatform.Tests.Integration;

public sealed class PostgresWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly PostgresFixture _postgres;

    public PostgresWebApplicationFactory(PostgresFixture postgres)
    {
        _postgres = postgres;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("IntegrationTests");
        builder.UseSetting("ConnectionStrings:DefaultConnection", _postgres.ConnectionString);
        builder.UseSetting("ConnectionStrings:Redis", string.Empty);
        builder.UseSetting("Auth:OtpRequestCooldownSeconds", "0");
    }
}
