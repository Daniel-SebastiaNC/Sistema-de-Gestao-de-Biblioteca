using DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Services;

namespace Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AutorController : ControllerBase
{
    private readonly IAutorService _service;

    public AutorController(IAutorService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResultDTO<AutorResponseDto>>> GetAllAutores([FromQuery] PaginationParamsDTO pagination)
    {
        var autores = await _service.GetPagedAutoresAsync(pagination);
        return Ok(autores);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AutorResponseDto>> GetAutorById(Guid id)
    {
        var autor = await _service.GetAutorByIdAsync(id);
        return Ok(autor);
    }

    [HttpPost]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Bibliotecario}")]
    public async Task<ActionResult<AutorResponseDto>> AddAutor(CriarAutorDto dto)
    {
        var autor = await _service.AddAutorAsync(dto);
        return CreatedAtAction(nameof(GetAutorById), new { id = autor.Id }, autor);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Bibliotecario}")]
    public async Task<ActionResult<AutorResponseDto>> UpadateAutor(Guid id, CriarAutorDto dto)
    {
        var autor = await _service.UpdateAutorAsync(id, dto);
        return Ok(autor);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Bibliotecario}")]
    public async Task<IActionResult> DeleteAutor(Guid id)
    {
        await _service.DeleteAutorAsync(id);
        return NoContent();
    }
}