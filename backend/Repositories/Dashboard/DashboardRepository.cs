using DataContext;
using DTO;
using Microsoft.EntityFrameworkCore;
using Models;

namespace Repository;

public class DashboardRepository : IDashboardRepository
{
    private readonly BibliotecaContext _contextDb;

    public DashboardRepository(BibliotecaContext contextDb)
    {
        _contextDb = contextDb;
    }

    public async Task<int> GetTotalLivrosAsync()
    {
        return await _contextDb.Livros.SumAsync(l => (int?)l.Quantidade) ?? 0;
    }

    public async Task<int> GetTotalUsuariosAtivosAsync()
    {
        return await _contextDb.Alunos.CountAsync();
    }

    public async Task<int> GetTotalEmprestimosAtivosAsync()
    {
        return await _contextDb.Emprestimos.CountAsync(e => e.Status == StatusEmprestimo.Ativo);
    }

    public async Task<int> GetTotalLivrosAtrasadosAsync()
    {
        var agora = DateTime.UtcNow;
        return await _contextDb.Emprestimos.CountAsync(e =>
            e.Status == StatusEmprestimo.Ativo && e.DataPrevistaDevolucao < agora);
    }

    public async Task<int> GetTotalReservasAtivasAsync()
    {
        return await _contextDb.Reservas.CountAsync(r => r.Status == StatusReserva.Ativa);
    }

    public async Task<DashboardDTO> GetDashboardStatsAsync()
    {
        var agora = DateTime.UtcNow;

        var totalLivros = await _contextDb.Livros.SumAsync(l => (int?)l.Quantidade) ?? 0;
        var totalUsuariosAtivos = await _contextDb.Alunos.CountAsync();
        var totalEmprestimosAtivos = await _contextDb.Emprestimos.CountAsync(e => e.Status == StatusEmprestimo.Ativo);
        var totalLivrosAtrasados = await _contextDb.Emprestimos.CountAsync(e =>
            e.Status == StatusEmprestimo.Ativo && e.DataPrevistaDevolucao < agora);
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