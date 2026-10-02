namespace MoviePlatform.Application.Admin;

public interface IAdminDashboardService
{
    Task<AdminDashboardStatsDto> GetStatsAsync(CancellationToken cancellationToken);
}
