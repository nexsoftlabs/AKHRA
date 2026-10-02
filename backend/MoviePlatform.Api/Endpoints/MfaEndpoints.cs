using MoviePlatform.Infrastructure.Identity;

namespace MoviePlatform.Api.Endpoints;

public static class MfaEndpoints
{
    public static RouteGroupBuilder MapMfaEndpoints(this RouteGroupBuilder api)
    {
        var mfa = api.MapGroup("/auth/mfa").RequireAuthorization();

        mfa.MapGet("/setup", async (AdminMfaService mfaService, HttpContext ctx) =>
        {
            var setup = await mfaService.GetSetupAsync(ctx.User);
            return setup is null ? Results.Unauthorized() : Results.Ok(setup);
        });

        mfa.MapPost("/setup/confirm", async (MfaConfirmRequest request, AdminMfaService mfaService, HttpContext ctx) =>
        {
            var ok = await mfaService.ConfirmSetupAsync(ctx.User, request.Code);
            return ok ? Results.Ok(new { status = "enabled" }) : Results.BadRequest(new { message = "Invalid code." });
        });

        mfa.MapPost("/verify", async (MfaConfirmRequest request, AdminMfaService mfaService, HttpContext ctx) =>
        {
            var ok = await mfaService.VerifyStepUpAsync(ctx.User, request.Code, ctx);
            return ok ? Results.Ok(new { status = "verified" }) : Results.BadRequest(new { message = "Invalid code." });
        });

        return api;
    }
}

public record MfaConfirmRequest(string Code);
