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
    public ActionResult<List<AlunoResponseDTO>> GetAllAlunos()
    {
        var alunos = _service.GetAllAlunos();
        return Ok(alunos);
    }

     [HttpGet("{id}")]
    public ActionResult<AlunoResponseDTO> GetAlunoById(Guid id)
    {
        var aluno = _service.GetAlunoById(id);
        return Ok(aluno);
    }

    [HttpPost]
    public ActionResult<AlunoResponseDTO> AddAluno(CriarAlunoDTO dto)
    {
        var aluno = _service.AddAluno(dto);
        return CreatedAtAction(nameof(GetAlunoById), new {id = aluno.Id}, aluno);
    }



    [HttpDelete("{id}")]
    public IActionResult DeleteAluno(Guid id)
    {
        _service.DeleteAluno(id);
        return NoContent();
    }
}