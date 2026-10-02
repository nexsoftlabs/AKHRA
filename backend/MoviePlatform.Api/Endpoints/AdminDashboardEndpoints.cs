using MoviePlatform.Application.Admin;
using MoviePlatform.Application.Authorization;

namespace MoviePlatform.Api.Endpoints;

public static class AdminDashboardEndpoints
{
    public static RouteGroupBuilder MapAdminDashboardEndpoints(this RouteGroupBuilder api)
    {
        var admin = api.MapGroup("/admin")
            .RequireAuthorization(AppPolicies.AdminPanel)
            .RequireAuthorization("AdminMfa");

        admin.MapGet("/dashboard/stats", async (IAdminDashboardService dashboard, CancellationToken ct) =>
            Results.Ok(await dashboard.GetStatsAsync(ct)));

        return api;
    }
}
