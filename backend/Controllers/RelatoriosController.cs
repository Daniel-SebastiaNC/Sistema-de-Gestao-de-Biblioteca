using DTO;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace Controllers;

[ApiController]
[Route("api/[controller]")]
public class RelatoriosController : ControllerBase
{
    private readonly IRelatorioService _relatorioService;

    public RelatoriosController(IRelatorioService relatorioService)
    {
        _relatorioService = relatorioService;
    }

    [HttpGet("populares")]
    public async Task<ActionResult<List<LivroPopularDTO>>> GetPopulares([FromQuery] int top = 10)
    {
        var populares = await _relatorioService.GetLivrosMaisPopularesAsync(top);
        return Ok(populares);
    }

    [HttpGet("atrasados")]
    public async Task<ActionResult<List<EmprestimoAtrasadoDTO>>> GetAtrasados()
    {
        var atrasados = await _relatorioService.GetEmprestimosAtrasadosAsync();
        return Ok(atrasados);
    }

    [HttpGet("historico")]
    public async Task<ActionResult<List<HistoricoTransacaoDTO>>> GetHistorico(
        [FromQuery] DateTime? dataInicio,
        [FromQuery] DateTime? dataFim)
    {
        var historico = await _relatorioService.GetHistoricoTransacoesAsync(dataInicio, dataFim);
        return Ok(historico);
    }
}
