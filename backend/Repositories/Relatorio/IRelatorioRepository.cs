using DTO;
using Models;

namespace Repository;

public interface IRelatorioRepository
{
    Task<List<LivroPopularDTO>> GetLivrosMaisPopularesAsync(int top = 10);
    Task<List<Emprestimo>> GetEmprestimosAtrasadosAsync(DateTime dataReferencia);
    Task<List<Emprestimo>> GetEmprestimosPorPeriodoAsync(DateTime inicio, DateTime fim);
    Task<List<Reserva>> GetReservasPorPeriodoAsync(DateTime inicio, DateTime fim);
}