using DTO;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace Controllers;

[ApiController]
[Route("api/[controller]")]
public class AlunoController : ControllerBase
{
    private readonly IAlunoService _service;

    public AlunoController(IAlunoService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResultDTO<AlunoResponseDTO>>> GetAllAlunos([FromQuery] PaginationParamsDTO pagination)
    {
        var alunos = await _service.GetPagedAlunosAsync(pagination);
        return Ok(alunos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AlunoResponseDTO>> GetAlunoById(Guid id)
    {
        var aluno = await _service.GetAlunoByIdAsync(id);
        return Ok(aluno);
    }

    [HttpPost]
    public async Task<ActionResult<AlunoResponseDTO>> AddAluno(CriarAlunoDTO dto)
    {
        var aluno = await _service.AddAlunoAsync(dto);
        return CreatedAtAction(nameof(GetAlunoById), new { id = aluno.Id }, aluno);
    }



    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAluno(Guid id)
    {
        await _service.DeleteAlunoAsync(id);
        return NoContent();
    }
}