using DTO;

namespace Services;

public interface IRelatorioService
{
    Task<List<LivroPopularDTO>> GetLivrosMaisPopularesAsync(int top = 10);
    Task<List<EmprestimoAtrasadoDTO>> GetEmprestimosAtrasadosAsync();
    Task<List<HistoricoTransacaoDTO>> GetHistoricoTransacoesAsync(DateTime? dataInicio, DateTime? dataFim);
}
