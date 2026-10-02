using Testcontainers.PostgreSql;

namespace MoviePlatform.Tests.Integration;

public sealed class PostgresFixture : IAsyncLifetime
{
    private PostgreSqlContainer? _postgres;

    public bool IsAvailable { get; private set; }

    public string ConnectionString =>
        _postgres?.GetConnectionString()
        ?? throw new InvalidOperationException("PostgreSQL test container is not available.");

    public async Task InitializeAsync()
    {
        try
        {
            _postgres = new PostgreSqlBuilder()
                .WithImage("postgres:16-alpine")
                .WithDatabase("movieplatform_test")
                .WithUsername("movieplatform")
                .WithPassword("movieplatform_test")
                .Build();

            await _postgres.StartAsync();
            IsAvailable = true;
        }
        catch (Exception)
        {
            IsAvailable = false;
        }
    }

    public async Task DisposeAsync()
    {
        if (_postgres is not null)
        {
            await _postgres.DisposeAsync();
        }
    }
}
