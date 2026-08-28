using DTO;

namespace Services;

public interface IDashboardService
{
    Task<DashboardDTO> GetDashboardStatsAsync();
}
