using DTO;

namespace Repository;

public interface IDashboardRepository
{
    Task<DashboardDTO> GetDashboardStatsAsync();
    Task<int> GetTotalLivrosAsync();
    Task<int> GetTotalUsuariosAtivosAsync();
    Task<int> GetTotalEmprestimosAtivosAsync();
    Task<int> GetTotalLivrosAtrasadosAsync();
    Task<int> GetTotalReservasAtivasAsync();
}