using MoviePlatform.Application.Catalog;

namespace MoviePlatform.Api.Endpoints;

public static class CatalogEndpoints
{
    public static RouteGroupBuilder MapCatalogEndpoints(this RouteGroupBuilder api)
    {
        var movies = api.MapGroup("/movies");

        movies.MapGet("/", async (string? q, string? genre, ICatalogService catalog, CancellationToken ct) =>
        {
            var list = await catalog.ListPublishedAsync(q, genre, ct);
            return Results.Ok(list);
        }).AllowAnonymous();

        movies.MapGet("/genres", async (ICatalogService catalog, CancellationToken ct) =>
            Results.Ok(await catalog.ListPublishedGenreSlugsAsync(ct))).AllowAnonymous();

        movies.MapGet("/{slug}", async (string slug, ICatalogService catalog, HttpContext ctx, CancellationToken ct) =>
        {
            var movie = await catalog.GetPublishedBySlugAsync(slug, ctx.User, ct);
            return movie is null ? Results.NotFound() : Results.Ok(movie);
        }).AllowAnonymous();

        return api;
    }
}
