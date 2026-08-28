using DTO;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace Controllers;

[ApiController]
[Route("api/[controller]")]
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
        var reserva = await _reservaService.AddReservaAsync(dto);
        return Created($"/api/reservas/{reserva.Id}", reserva);
    }

    [HttpGet("fila/{livroId}")]
    public async Task<ActionResult<List<ReservaResponseDTO>>> GetFilaEspera(Guid livroId)
    {
        var fila = await _reservaService.GetFilaEsperaAsync(livroId);
        return Ok(fila);
    }
}
