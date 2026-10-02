using MoviePlatform.Application.Commerce;

namespace MoviePlatform.Api.Endpoints;

public static class CheckoutEndpoints
{
    public static RouteGroupBuilder MapCheckoutEndpoints(this RouteGroupBuilder api)
    {
        var checkout = api.MapGroup("/checkout");

        checkout.MapPost("/movies/{slug}", CreateMovieCheckoutAsync).RequireAuthorization();
        checkout.MapPost("/verify", VerifyAsync).RequireAuthorization();
        checkout.MapPost("/webhooks/razorpay", RazorpayWebhookAsync).AllowAnonymous();

        return api;
    }

    private static async Task<IResult> CreateMovieCheckoutAsync(
        string slug,
        ICheckoutService checkoutService,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var result = await checkoutService.CreateMovieCheckoutAsync(slug, httpContext.User, cancellationToken);
        return ToResult(result, checkout: true);
    }

    private static async Task<IResult> VerifyAsync(
        VerifyCheckoutRequest request,
        ICheckoutService checkoutService,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var result = await checkoutService.VerifyPaymentAsync(request, httpContext.User, cancellationToken);
        return ToResult(result, checkout: false);
    }

    private static async Task<IResult> RazorpayWebhookAsync(
        HttpContext httpContext,
        ICheckoutService checkoutService,
        CancellationToken cancellationToken)
    {
        httpContext.Request.EnableBuffering();
        using var reader = new StreamReader(httpContext.Request.Body);
        var rawBody = await reader.ReadToEndAsync(cancellationToken);

        var signature = httpContext.Request.Headers["X-Razorpay-Signature"].ToString();
        var ok = await checkoutService.ProcessWebhookAsync(rawBody, signature, cancellationToken);
        return ok ? Results.Ok(new { status = "ok" }) : Results.BadRequest(new { status = "invalid" });
    }

    private static IResult ToResult(CheckoutResult result, bool checkout)
    {
        if (result.Succeeded)
        {
            if (checkout && result.Checkout is not null)
            {
                return Results.Ok(result.Checkout);
            }

            if (!checkout && result.Verification is not null)
            {
                return Results.Ok(result.Verification);
            }
        }

        var status = result.ErrorCode switch
        {
            "unauthorized" => StatusCodes.Status401Unauthorized,
            "forbidden" => StatusCodes.Status403Forbidden,
            "movie_not_found" or "payment_not_found" => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status400BadRequest,
        };

        return Results.Json(
            new { error = result.ErrorCode, message = result.ErrorMessage },
            statusCode: status);
    }
}
