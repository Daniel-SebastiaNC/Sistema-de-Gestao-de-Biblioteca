using DataContext;
using DTO;
using Microsoft.EntityFrameworkCore;
using Models;

namespace Repository;

public class RelatorioRepository : IRelatorioRepository
{
    private readonly BibliotecaContext _contextDb;

    public RelatorioRepository(BibliotecaContext contextDb)
    {
        _contextDb = contextDb;
    }

    public async Task<List<LivroPopularDTO>> GetLivrosMaisPopularesAsync(int top = 10)
    {
        var topLivros = await _contextDb.Emprestimos
            .GroupBy(e => e.LivroId)
            .Select(g => new
            {
                LivroId = g.Key,
                TotalEmprestimos = g.Count()
            })
            .OrderByDescending(x => x.TotalEmprestimos)
            .Take(top)
            .ToListAsync();

        var ids = topLivros.Select(t => t.LivroId).ToList();
        var livrosInfo = await _contextDb.Livros
            .Include(l => l.Autor)
            .Where(l => ids.Contains(l.Id))
            .ToDictionaryAsync(l => l.Id);

        var result = new List<LivroPopularDTO>();
        foreach (var item in topLivros)
        {
            if (livrosInfo.TryGetValue(item.LivroId, out var livro))
            {
                result.Add(new LivroPopularDTO
                {
                    LivroId = item.LivroId,
                    Titulo = livro.Titulo,
                    AutorNome = livro.Autor != null ? livro.Autor.Nome : "Desconhecido",
                    TotalEmprestimos = item.TotalEmprestimos
                });
            }
        }

        return result;
    }

    public async Task<List<Emprestimo>> GetEmprestimosAtrasadosAsync(DateTime dataReferencia)
    {
        return await _contextDb.Emprestimos
            .Include(e => e.Aluno)
            .Include(e => e.Livro)
            .Where(e => e.Status == StatusEmprestimo.Ativo && e.DataPrevistaDevolucao < dataReferencia)
            .ToListAsync();
    }

    public async Task<List<Emprestimo>> GetEmprestimosPorPeriodoAsync(DateTime inicio, DateTime fim)
    {
        return await _contextDb.Emprestimos
            .Include(e => e.Aluno)
            .Include(e => e.Livro)
            .Where(e => e.DataEmprestimo >= inicio && e.DataEmprestimo <= fim)
            .ToListAsync();
    }

    public async Task<List<Reserva>> GetReservasPorPeriodoAsync(DateTime inicio, DateTime fim)
    {
        return await _contextDb.Reservas
            .Include(r => r.Aluno)
            .Include(r => r.Livro)
            .Where(r => r.DataReserva >= inicio && r.DataReserva <= fim)
            .ToListAsync();
    }
}