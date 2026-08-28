using DTO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Repository;

namespace Services;

public class DashboardService : IDashboardService
{
    private readonly IDashboardRepository _dashboardRepository;
    private readonly ILogger<DashboardService> _logger;
    private readonly ICacheService? _cacheService;

    public DashboardService(
        IDashboardRepository dashboardRepository,
        ILogger<DashboardService>? logger = null,
        ICacheService? cacheService = null)
    {
        _dashboardRepository = dashboardRepository;
        _logger = logger ?? NullLogger<DashboardService>.Instance;
        _cacheService = cacheService;
    }

    public async Task<DashboardDTO> GetDashboardStatsAsync()
    {
        const string cacheKey = "dashboard:stats";

        if (_cacheService != null)
        {
            var cachedStats = await _cacheService.GetAsync<DashboardDTO>(cacheKey);
            if (cachedStats != null)
            {
                return cachedStats;
            }
        }

        _logger.LogInformation("Calculando estatísticas consolidadas do Dashboard através do repositório");

        var stats = await _dashboardRepository.GetDashboardStatsAsync();

        if (_cacheService != null)
        {
            await _cacheService.SetAsync(cacheKey, stats, TimeSpan.FromMinutes(2));
        }

        return stats;
    }
}
