using MoviePlatform.Application.Subscriptions;

namespace MoviePlatform.Api.Endpoints;

public static class SubscriptionEndpoints
{
    public static RouteGroupBuilder MapSubscriptionEndpoints(this RouteGroupBuilder api)
    {
        var plans = api.MapGroup("/subscriptions");

        plans.MapGet("/plans", async (ISubscriptionService subscriptions, CancellationToken ct) =>
            Results.Ok(await subscriptions.ListPlansAsync(ct))).AllowAnonymous();

        plans.MapPost("/plans/{slug}/checkout", async (
            string slug,
            ISubscriptionService subscriptions,
            HttpContext ctx,
            CancellationToken ct) =>
        {
            var result = await subscriptions.CreatePlanCheckoutAsync(slug, ctx.User, ct);
            if (result.Succeeded && result.Checkout is not null)
            {
                return Results.Ok(result.Checkout);
            }

            return Results.Json(
                new { error = result.ErrorCode, message = result.ErrorMessage },
                statusCode: StatusCodes.Status400BadRequest);
        }).RequireAuthorization();

        return api;
    }
}
