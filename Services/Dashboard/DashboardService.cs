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

    public DashboardService(
        BibliotecaContext contextDb,
        ILogger<DashboardService>? logger = null)
    {
        _contextDb = contextDb;
        _logger = logger ?? NullLogger<DashboardService>.Instance;
    }

    public async Task<DashboardDTO> GetDashboardStatsAsync()
    {
        _logger.LogInformation("Calculando estatísticas consolidadas do Dashboard");

        var totalLivros = await _contextDb.Livros.SumAsync(l => (int?)l.Quantidade) ?? 0;
        var totalUsuariosAtivos = await _contextDb.Alunos.CountAsync();
        var totalEmprestimosAtivos = await _contextDb.Emprestimos.CountAsync(e => e.Status == StatusEmprestimo.Ativo);
        var totalLivrosAtrasados = await _contextDb.Emprestimos.CountAsync(e =>
            e.Status == StatusEmprestimo.Ativo && e.DataPrevistaDevolucao < DateTime.Now);
        var totalReservasAtivas = await _contextDb.Reservas.CountAsync(r => r.Status == StatusReserva.Ativa);

        return new DashboardDTO
        {
            TotalLivros = totalLivros,
            TotalUsuariosAtivos = totalUsuariosAtivos,
            TotalEmprestimosAtivos = totalEmprestimosAtivos,
            TotalLivrosAtrasados = totalLivrosAtrasados,
            TotalReservasAtivas = totalReservasAtivas
        };
    }
}
