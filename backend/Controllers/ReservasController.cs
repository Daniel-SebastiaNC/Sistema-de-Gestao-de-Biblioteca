using System.Security.Claims;
using DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Services;

namespace Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReservasController : ControllerBase
{
    private readonly IReservaService _reservaService;

    public ReservasController(IReservaService reservaService)
    {
        _reservaService = reservaService;
    }

    [HttpPost]
    public async Task<ActionResult<ReservaResponseDTO>> AddReserva([FromBody] CriarReservaDTO dto)
    {
        // Se for perfil Aluno, garante a auto-reserva associando o ID do aluno autenticado
        if (User.IsInRole(Roles.Aluno))
        {
            var alunoIdClaim = User.FindFirst("alunoId")?.Value;
            if (string.IsNullOrEmpty(alunoIdClaim) || !Guid.TryParse(alunoIdClaim, out var alunoId))
            {
                return BadRequest(new { message = "Aluno não identificado no token." });
            }
            dto.AlunoId = alunoId;
        }

        var reserva = await _reservaService.AddReservaAsync(dto);
        return Created($"/api/reservas/{reserva.Id}", reserva);
    }

    [HttpGet("fila/{livroId}")]
    public async Task<ActionResult<List<ReservaResponseDTO>>> GetFilaEspera(Guid livroId)
    {
        var fila = await _reservaService.GetFilaEsperaAsync(livroId);
        return Ok(fila);
    }

    [HttpGet("minhas")]
    [Authorize(Roles = Roles.Aluno)]
    public async Task<ActionResult<List<ReservaResponseDTO>>> GetMinhasReservas()
    {
        var alunoIdClaim = User.FindFirst("alunoId")?.Value;
        if (string.IsNullOrEmpty(alunoIdClaim) || !Guid.TryParse(alunoIdClaim, out var alunoId))
        {
            return BadRequest(new { message = "Aluno não identificado no token." });
        }

        var reservas = await _reservaService.GetByAlunoIdAsync(alunoId);
        return Ok(reservas);
    }
}
