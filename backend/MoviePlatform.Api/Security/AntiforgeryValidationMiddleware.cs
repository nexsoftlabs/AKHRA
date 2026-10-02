using Microsoft.AspNetCore.Antiforgery;

namespace MoviePlatform.Api.Security;

public sealed class AntiforgeryValidationMiddleware(RequestDelegate next, IAntiforgery antiforgery)
{
    private static readonly HashSet<string> SafeMethods = new(StringComparer.OrdinalIgnoreCase)
    {
        HttpMethods.Get,
        HttpMethods.Head,
        HttpMethods.Options,
        HttpMethods.Trace,
    };

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;
        if (!path.StartsWith("/api/", StringComparison.OrdinalIgnoreCase) ||
            SafeMethods.Contains(context.Request.Method) ||
            path.Contains("/webhooks/", StringComparison.OrdinalIgnoreCase) ||
            path.Equals("/api/v1/antiforgery/token", StringComparison.OrdinalIgnoreCase))
        {
            await next(context);
            return;
        }

        try
        {
            await antiforgery.ValidateRequestAsync(context);
        }
        catch (AntiforgeryValidationException)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "invalid_csrf",
                message = "Missing or invalid CSRF token. Call GET /api/v1/antiforgery/token first.",
            });
            return;
        }

        await next(context);
    }
}
