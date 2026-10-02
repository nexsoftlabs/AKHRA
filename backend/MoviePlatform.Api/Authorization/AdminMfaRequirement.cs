using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MoviePlatform.Infrastructure.Identity;

namespace MoviePlatform.Api.Authorization;

public sealed class AdminMfaRequirement : IAuthorizationRequirement;

public sealed class AdminMfaHandler(
    AdminMfaService mfaService,
    IHttpContextAccessor httpContextAccessor,
    IHostEnvironment environment,
    IOptions<AdminMfaOptions> options) : AuthorizationHandler<AdminMfaRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AdminMfaRequirement requirement)
    {
        if (!await mfaService.RequiresStepUpAsync(context.User))
        {
            context.Succeed(requirement);
            return;
        }

        if (environment.IsDevelopment() && options.Value.SkipStepUpInDevelopment)
        {
            context.Succeed(requirement);
            return;
        }

        var http = httpContextAccessor.HttpContext;
        if (http is null)
        {
            return;
        }

        if (mfaService.HasValidStepUpCookie(http))
        {
            context.Succeed(requirement);
        }
    }
}
