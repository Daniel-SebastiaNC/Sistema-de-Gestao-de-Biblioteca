using DTO;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace Controllers;

[ApiController]
[Route("api/[controller]")]
public class AutorController : ControllerBase
{
    private readonly IAutorService _service; 

    public AutorController(IAutorService service)
    {
        _service = service;
    }

    [HttpGet]
    public ActionResult<List<AlunoResponseDTO>> GetAllAutores()
    {
        var alunos = _service.GetAllAutores();
        return Ok(alunos);
    }

     [HttpGet("{id}")]
    public ActionResult<AlunoResponseDTO> GetAutorById(Guid id)
    {
        var aluno = _service.GetAutorById(id);
        return Ok(aluno);
    }

    [HttpPost]
    public ActionResult<AlunoResponseDTO> AddAutor(CriarAutorDto dto)
    {
        var aluno = _service.AddAutor(dto);
        return CreatedAtAction(nameof(GetAutorById), new {id = aluno.Id}, aluno);
    }

    [HttpPut("{id}")]
    public ActionResult<AutorResponseDto> UpadateAutor(Guid id, CriarAutorDto dto)
    {
        var autor = _service.UpdateAutor(id, dto);
        return Ok(autor);
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteAutor(Guid id)
    {
        _service.DeleteAutor(id);
        return NoContent();
    }
}