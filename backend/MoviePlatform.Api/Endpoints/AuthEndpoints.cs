using FluentValidation;
using Microsoft.Extensions.Options;
using MoviePlatform.Api.Validation;
using MoviePlatform.Application.Identity;

namespace MoviePlatform.Api.Endpoints;

public static class AuthEndpoints
{
    public static RouteGroupBuilder MapAuthEndpoints(this RouteGroupBuilder api)
    {
        var auth = api.MapGroup("/auth");

        auth.MapPost("/register", RegisterAsync).AllowAnonymous();
        auth.MapPost("/login", LoginAsync).AllowAnonymous();
        auth.MapGet("/google/config", GoogleConfigAsync).AllowAnonymous();
        auth.MapPost("/google", GoogleAsync).AllowAnonymous();
        auth.MapPost("/google/link", LinkGoogleAsync).RequireAuthorization();
        auth.MapPost("/otp/request", OtpRequestAsync).AllowAnonymous();
        auth.MapPost("/otp/verify", OtpVerifyAsync).AllowAnonymous();
        auth.MapPost("/refresh", RefreshAsync).AllowAnonymous();
        auth.MapPost("/logout", LogoutAsync).RequireAuthorization();
        auth.MapPost("/password/reset", PasswordResetAsync).AllowAnonymous();
        auth.MapPost("/password/reset/confirm", PasswordResetConfirmAsync).AllowAnonymous();
        auth.MapPost("/email/confirm", ConfirmEmailAsync).AllowAnonymous();
        auth.MapPost("/email/resend", ResendEmailConfirmationAsync).RequireAuthorization();

        api.MapGet("/users/me", GetMeAsync).RequireAuthorization();
        api.MapDelete("/users/me", DeleteMeAsync).RequireAuthorization();

        return api;
    }

    private static async Task<IResult> RegisterAsync(
        RegisterRequest request,
        IValidator<RegisterRequest> validator,
        IAuthService authService,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var validation = await ValidationExtensions.ValidateAsync(request, validator, cancellationToken);
        if (validation is not null)
        {
            return validation;
        }

        var result = await authService.RegisterAsync(request, httpContext, cancellationToken);
        return ToResult(result);
    }

    private static async Task<IResult> LoginAsync(
        LoginRequest request,
        IValidator<LoginRequest> validator,
        IAuthService authService,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var validation = await ValidationExtensions.ValidateAsync(request, validator, cancellationToken);
        if (validation is not null)
        {
            return validation;
        }

        var result = await authService.LoginAsync(request, httpContext, cancellationToken);
        return ToResult(result);
    }

    private static IResult GoogleConfigAsync(IOptions<AuthOptions> authOptions)
    {
        var clientId = authOptions.Value.GoogleClientId?.Trim();
        var enabled = !string.IsNullOrEmpty(clientId);
        return Results.Ok(new { enabled, clientId = enabled ? clientId : null });
    }

    private static async Task<IResult> GoogleAsync(
        GoogleSignInRequest request,
        IValidator<GoogleSignInRequest> validator,
        IAuthService authService,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var validation = await ValidationExtensions.ValidateAsync(request, validator, cancellationToken);
        if (validation is not null)
        {
            return validation;
        }

        var result = await authService.GoogleSignInAsync(request, httpContext, cancellationToken);
        return ToResult(result);
    }

    private static async Task<IResult> LinkGoogleAsync(
        GoogleSignInRequest request,
        IValidator<GoogleSignInRequest> validator,
        IAuthService authService,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var validation = await ValidationExtensions.ValidateAsync(request, validator, cancellationToken);
        if (validation is not null)
        {
            return validation;
        }

        var result = await authService.LinkGoogleAsync(request, httpContext.User, cancellationToken);
        return ToResult(result);
    }

    private static async Task<IResult> OtpRequestAsync(
        OtpRequest request,
        IValidator<OtpRequest> validator,
        IOtpAuthService otpAuthService,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var validation = await ValidationExtensions.ValidateAsync(request, validator, cancellationToken);
        if (validation is not null)
        {
            return validation;
        }

        var ip = httpContext.Connection.RemoteIpAddress?.ToString();
        var result = await otpAuthService.RequestOtpAsync(request, ip, cancellationToken);
        if (!result.Succeeded)
        {
            if (result.RetryAfterSeconds is int retry)
            {
                httpContext.Response.Headers.RetryAfter = retry.ToString();
            }

            return Results.BadRequest(new { error = result.ErrorCode, message = result.ErrorMessage });
        }

        return Results.Ok(new { status = "sent" });
    }

    private static async Task<IResult> OtpVerifyAsync(
        OtpVerifyRequest request,
        IValidator<OtpVerifyRequest> validator,
        IOtpAuthService otpAuthService,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var validation = await ValidationExtensions.ValidateAsync(request, validator, cancellationToken);
        if (validation is not null)
        {
            return validation;
        }

        var result = await otpAuthService.VerifyOtpAsync(request, httpContext, cancellationToken);
        return ToResult(result);
    }

    private static async Task<IResult> RefreshAsync(
        IAuthService authService,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var result = await authService.RefreshSessionAsync(httpContext, cancellationToken);
        return ToResult(result);
    }

    private static async Task<IResult> LogoutAsync(
        IAuthService authService,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        await authService.LogoutAsync(httpContext, cancellationToken);
        return Results.Ok(new { status = "signed_out" });
    }

    private static async Task<IResult> PasswordResetAsync(
        PasswordResetRequest request,
        IValidator<PasswordResetRequest> validator,
        IAuthService authService,
        CancellationToken cancellationToken)
    {
        var validation = await ValidationExtensions.ValidateAsync(request, validator, cancellationToken);
        if (validation is not null)
        {
            return validation;
        }

        await authService.RequestPasswordResetAsync(request, cancellationToken);
        return Results.Ok(new { status = "if_account_exists_email_sent" });
    }

    private static async Task<IResult> PasswordResetConfirmAsync(
        PasswordResetConfirmRequest request,
        IValidator<PasswordResetConfirmRequest> validator,
        IAuthService authService,
        CancellationToken cancellationToken)
    {
        var validation = await ValidationExtensions.ValidateAsync(request, validator, cancellationToken);
        if (validation is not null)
        {
            return validation;
        }

        try
        {
            await authService.ConfirmPasswordResetAsync(request, cancellationToken);
            return Results.Ok(new { status = "password_updated" });
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { error = "reset_failed", message = ex.Message });
        }
    }

    private static async Task<IResult> ConfirmEmailAsync(
        ConfirmEmailRequest request,
        IAuthService authService,
        CancellationToken cancellationToken)
    {
        var result = await authService.ConfirmEmailAsync(request, cancellationToken);
        return ToResult(result);
    }

    private static async Task<IResult> ResendEmailConfirmationAsync(
        IAuthService authService,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        try
        {
            await authService.ResendEmailConfirmationAsync(httpContext.User, cancellationToken);
            return Results.Ok(new { status = "sent" });
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { error = "resend_failed", message = ex.Message });
        }
    }

    private static async Task<IResult> GetMeAsync(
        IAuthService authService,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var profile = await authService.GetProfileAsync(httpContext.User, cancellationToken);
        return profile is null ? Results.Unauthorized() : Results.Ok(profile);
    }

    private static async Task<IResult> DeleteMeAsync(
        IAuthService authService,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        try
        {
            await authService.DeleteAccountAsync(httpContext.User, httpContext, cancellationToken);
            return Results.Ok(new { status = "deleted" });
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { error = "delete_failed", message = ex.Message });
        }
    }

    private static IResult ToResult(AuthResult result)
    {
        if (result.Succeeded && result.User is not null)
        {
            return Results.Ok(result.User);
        }

        return Results.BadRequest(new { error = result.ErrorCode, message = result.ErrorMessage });
    }
}
