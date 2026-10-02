using Microsoft.AspNetCore.Authorization;
using MoviePlatform.Application.Authorization;
using MoviePlatform.Application.Catalog;

namespace MoviePlatform.Api.Endpoints;

public static class AdminCatalogEndpoints
{
    public static RouteGroupBuilder MapAdminCatalogEndpoints(this RouteGroupBuilder api)
    {
        var admin = api.MapGroup("/admin")
            .RequireAuthorization(AppPolicies.ContentManagement)
            .RequireAuthorization("AdminMfa");

        var movies = admin.MapGroup("/movies");

        movies.MapGet("/", async (string? q, string? status, IAdminCatalogService catalog, CancellationToken ct) =>
        {
            var list = await catalog.ListAsync(q, status, ct);
            return Results.Ok(list);
        });

        movies.MapGet("/genres", async (IAdminCatalogService catalog, CancellationToken ct) =>
            Results.Ok(await catalog.ListGenreSlugsAsync(ct)));

        movies.MapGet("/{id:guid}", async (Guid id, IAdminCatalogService catalog, CancellationToken ct) =>
        {
            var movie = await catalog.GetAsync(id, ct);
            return movie is null ? Results.NotFound() : Results.Ok(movie);
        });

        movies.MapPost("/", async (CreateMovieRequest request, IAdminCatalogService catalog, HttpContext ctx, CancellationToken ct) =>
        {
            try
            {
                var created = await catalog.CreateAsync(request, ctx.User, ct);
                return Results.Created($"/api/v1/admin/movies/{created.Id}", created);
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }
        });

        movies.MapPut("/{id:guid}", async (Guid id, UpdateMovieRequest request, IAdminCatalogService catalog, CancellationToken ct) =>
        {
            try
            {
                var updated = await catalog.UpdateAsync(id, request, ct);
                return updated is null ? Results.NotFound() : Results.Ok(updated);
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }
        });

        movies.MapPost("/{id:guid}/publish", async (Guid id, IAdminCatalogService catalog, CancellationToken ct) =>
        {
            try
            {
                var ok = await catalog.PublishAsync(id, ct);
                return ok ? Results.Ok(new { status = "published" }) : Results.NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }
        });

        movies.MapPost("/{id:guid}/unpublish", async (Guid id, IAdminCatalogService catalog, CancellationToken ct) =>
        {
            var ok = await catalog.UnpublishAsync(id, ct);
            return ok ? Results.Ok(new { status = "unpublished" }) : Results.NotFound();
        });

        return api;
    }
}
