using DataContext;
using DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Models;

namespace Services;

public class DashboardService : IDashboardService
{
    private readonly BibliotecaContext _contextDb;
    private readonly ILogger<DashboardService> _logger;
    private readonly ICacheService? _cacheService;

    public DashboardService(
        BibliotecaContext contextDb,
        ILogger<DashboardService>? logger = null,
        ICacheService? cacheService = null)
    {
        _contextDb = contextDb;
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

        _logger.LogInformation("Calculando estatísticas consolidadas do Dashboard no banco de dados");

        var totalLivros = await _contextDb.Livros.SumAsync(l => (int?)l.Quantidade) ?? 0;
        var totalUsuariosAtivos = await _contextDb.Alunos.CountAsync();
        var totalEmprestimosAtivos = await _contextDb.Emprestimos.CountAsync(e => e.Status == StatusEmprestimo.Ativo);
        var totalLivrosAtrasados = await _contextDb.Emprestimos.CountAsync(e =>
            e.Status == StatusEmprestimo.Ativo && e.DataPrevistaDevolucao < DateTime.Now);
        var totalReservasAtivas = await _contextDb.Reservas.CountAsync(r => r.Status == StatusReserva.Ativa);

        var stats = new DashboardDTO
        {
            TotalLivros = totalLivros,
            TotalUsuariosAtivos = totalUsuariosAtivos,
            TotalEmprestimosAtivos = totalEmprestimosAtivos,
            TotalLivrosAtrasados = totalLivrosAtrasados,
            TotalReservasAtivas = totalReservasAtivas
        };

        if (_cacheService != null)
        {
            await _cacheService.SetAsync(cacheKey, stats, TimeSpan.FromMinutes(2));
        }

        return stats;
    }
}
