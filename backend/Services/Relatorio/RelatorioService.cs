using DTO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Models;
using Repository;

namespace Services;

public class RelatorioService : IRelatorioService
{
    private readonly IRelatorioRepository _relatorioRepository;
    private readonly ILogger<RelatorioService> _logger;
    private readonly ICacheService? _cacheService;

    public RelatorioService(
        IRelatorioRepository relatorioRepository,
        ILogger<RelatorioService>? logger = null,
        ICacheService? cacheService = null)
    {
        _relatorioRepository = relatorioRepository;
        _logger = logger ?? NullLogger<RelatorioService>.Instance;
        _cacheService = cacheService;
    }

    public async Task<List<LivroPopularDTO>> GetLivrosMaisPopularesAsync(int top = 10)
    {
        var cacheKey = $"relatorios:populares:{top}";

        if (_cacheService != null)
        {
            var cachedReport = await _cacheService.GetAsync<List<LivroPopularDTO>>(cacheKey);
            if (cachedReport != null)
            {
                return cachedReport;
            }
        }

        _logger.LogInformation("Gerando relatório de livros mais populares (Top {Top}) via repositório", top);

        var result = await _relatorioRepository.GetLivrosMaisPopularesAsync(top);

        if (_cacheService != null)
        {
            await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5));
        }

        return result;
    }

    public async Task<List<EmprestimoAtrasadoDTO>> GetEmprestimosAtrasadosAsync()
    {
        _logger.LogInformation("Gerando relatório de empréstimos atrasados");

        var hoje = DateTime.UtcNow;
        var atrasados = await _relatorioRepository.GetEmprestimosAtrasadosAsync(hoje);

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

        var inicio = dataInicio.HasValue ? DateTime.SpecifyKind(dataInicio.Value, DateTimeKind.Utc) : DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc);
        var fim = dataFim.HasValue ? DateTime.SpecifyKind(dataFim.Value, DateTimeKind.Utc) : DateTime.SpecifyKind(DateTime.MaxValue, DateTimeKind.Utc);

        var emprestimos = await _relatorioRepository.GetEmprestimosPorPeriodoAsync(inicio, fim);
        var reservas = await _relatorioRepository.GetReservasPorPeriodoAsync(inicio, fim);

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
