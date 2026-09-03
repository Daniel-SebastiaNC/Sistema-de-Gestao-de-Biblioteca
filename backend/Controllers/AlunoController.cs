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
public class AlunoController : ControllerBase
{
    private readonly IAlunoService _service;

    public AlunoController(IAlunoService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Bibliotecario}")]
    public async Task<ActionResult<PagedResultDTO<AlunoResponseDTO>>> GetAllAlunos([FromQuery] PaginationParamsDTO pagination)
    {
        var alunos = await _service.GetPagedAlunosAsync(pagination);
        return Ok(alunos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AlunoResponseDTO>> GetAlunoById(Guid id)
    {
        if (User.IsInRole(Roles.Aluno))
        {
            var alunoIdClaim = User.FindFirst("alunoId")?.Value;
            if (string.IsNullOrEmpty(alunoIdClaim) || !Guid.TryParse(alunoIdClaim, out var alunoIdLogado) || alunoIdLogado != id)
            {
                return Forbid();
            }
        }

        var aluno = await _service.GetAlunoByIdAsync(id);
        return Ok(aluno);
    }

    [HttpGet("perfil")]
    [Authorize(Roles = Roles.Aluno)]
    public async Task<ActionResult<AlunoResponseDTO>> GetMeuPerfil()
    {
        var alunoIdClaim = User.FindFirst("alunoId")?.Value;
        if (string.IsNullOrEmpty(alunoIdClaim) || !Guid.TryParse(alunoIdClaim, out var alunoId))
        {
            return BadRequest(new { message = "Aluno não identificado no token." });
        }

        var aluno = await _service.GetAlunoByIdAsync(alunoId);
        return Ok(aluno);
    }

    [HttpPost]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Bibliotecario}")]
    public async Task<ActionResult<AlunoResponseDTO>> AddAluno(CriarAlunoDTO dto)
    {
        var aluno = await _service.AddAlunoAsync(dto);
        return CreatedAtAction(nameof(GetAlunoById), new { id = aluno.Id }, aluno);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> DeleteAluno(Guid id)
    {
        await _service.DeleteAlunoAsync(id);
        return NoContent();
    }
}