using DataContext;
using DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Models;

namespace Services;

public class RelatorioService : IRelatorioService
{
    private readonly BibliotecaContext _contextDb;
    private readonly ILogger<RelatorioService> _logger;

    public RelatorioService(
        BibliotecaContext contextDb,
        ILogger<RelatorioService>? logger = null)
    {
        _contextDb = contextDb;
        _logger = logger ?? NullLogger<RelatorioService>.Instance;
    }

    public async Task<List<LivroPopularDTO>> GetLivrosMaisPopularesAsync(int top = 10)
    {
        _logger.LogInformation("Gerando relatório de livros mais populares (Top {Top})", top);

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

    public async Task<List<EmprestimoAtrasadoDTO>> GetEmprestimosAtrasadosAsync()
    {
        _logger.LogInformation("Gerando relatório de empréstimos atrasados");

        var hoje = DateTime.Now;
        var atrasados = await _contextDb.Emprestimos
            .Include(e => e.Aluno)
            .Include(e => e.Livro)
            .Where(e => e.Status == StatusEmprestimo.Ativo && e.DataPrevistaDevolucao < hoje)
            .ToListAsync();

        const decimal valorMultaPorDia = 2.00m;

        return atrasados.Select(e =>
        {
            int diasAtraso = (int)Math.Ceiling((hoje - e.DataPrevistaDevolucao).TotalDays);
            decimal multaEstimada = diasAtraso * valorMultaPorDia;

            return new EmprestimoAtrasadoDTO
            {
                EmprestimoId = e.Id,
                AlunoId = e.AlunoId,
                AlunoNome = e.Aluno != null ? e.Aluno.Nome : "Desconhecido",
                AlunoMatricula = e.Aluno != null ? e.Aluno.Matricula : string.Empty,
                LivroId = e.LivroId,
                LivroTitulo = e.Livro != null ? e.Livro.Titulo : "Desconhecido",
                DataEmprestimo = e.DataEmprestimo,
                DataPrevistaDevolucao = e.DataPrevistaDevolucao,
                DiasAtraso = diasAtraso,
                MultaEstimada = multaEstimada
            };
        }).OrderByDescending(x => x.DiasAtraso).ToList();
    }

    public async Task<List<HistoricoTransacaoDTO>> GetHistoricoTransacoesAsync(DateTime? dataInicio, DateTime? dataFim)
    {
        _logger.LogInformation("Gerando relatório de histórico de transações de {DataInicio} até {DataFim}", dataInicio, dataFim);

        var inicio = dataInicio ?? DateTime.MinValue;
        var fim = dataFim ?? DateTime.MaxValue;

        var emprestimos = await _contextDb.Emprestimos
            .Include(e => e.Aluno)
            .Include(e => e.Livro)
            .Where(e => e.DataEmprestimo >= inicio && e.DataEmprestimo <= fim)
            .ToListAsync();

        var reservas = await _contextDb.Reservas
            .Include(r => r.Aluno)
            .Include(r => r.Livro)
            .Where(r => r.DataReserva >= inicio && r.DataReserva <= fim)
            .ToListAsync();

        var resultado = new List<HistoricoTransacaoDTO>();

        foreach (var e in emprestimos)
        {
            resultado.Add(new HistoricoTransacaoDTO
            {
                Id = e.Id,
                Tipo = e.Status == StatusEmprestimo.Devolvido ? "Devolução" : "Empréstimo",
                AlunoNome = e.Aluno != null ? e.Aluno.Nome : "Desconhecido",
                LivroTitulo = e.Livro != null ? e.Livro.Titulo : "Desconhecido",
                DataEvento = e.DataDevolucao ?? e.DataEmprestimo,
                Status = e.Status.ToString()
            });
        }

        foreach (var r in reservas)
        {
            resultado.Add(new HistoricoTransacaoDTO
            {
                Id = r.Id,
                Tipo = "Reserva",
                AlunoNome = r.Aluno != null ? r.Aluno.Nome : "Desconhecido",
                LivroTitulo = r.Livro != null ? r.Livro.Titulo : "Desconhecido",
                DataEvento = r.DataReserva,
                Status = r.Status.ToString()
            });
        }

        return resultado.OrderByDescending(x => x.DataEvento).ToList();
    }
}
