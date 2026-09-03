using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using DTO;
using Services;

namespace Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EmprestimosController : ControllerBase
    {
        private readonly IEmprestimoService _emprestimoService;

        public EmprestimosController(IEmprestimoService emprestimoService)
        {
            _emprestimoService = emprestimoService;
        }

        [HttpPost]
        [Authorize(Roles = $"{Roles.Admin},{Roles.Bibliotecario}")]
        public async Task<IActionResult> AddEmprestimo([FromBody] CriarEmprestimoDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var emprestimo = await _emprestimoService.AddEmprestimoAsync(dto);
            return Created($"/api/emprestimos/{emprestimo.Id}", emprestimo);
        }

        [HttpPut("{id}/devolucao")]
        [Authorize(Roles = $"{Roles.Admin},{Roles.Bibliotecario}")]
        public async Task<IActionResult> ReturnEmprestimo(Guid id)
        {
            var emprestimoAtualizado = await _emprestimoService.ReturnEmprestimoAsync(id);
            return Ok(emprestimoAtualizado);
        }

        [HttpPost("devolver")]
        [Authorize(Roles = $"{Roles.Admin},{Roles.Bibliotecario}")]
        public async Task<ActionResult<DevolucaoResponseDTO>> Devolver([FromBody] DevolverEmprestimoDTO dto)
        {
            var devolucao = await _emprestimoService.DevolverComCalculoMultaAsync(dto);
            return Ok(devolucao);
        }

        [HttpGet]
        [Authorize(Roles = $"{Roles.Admin},{Roles.Bibliotecario}")]
        public async Task<ActionResult<PagedResultDTO<EmprestimoResponseDTO>>> GetAll([FromQuery] PaginationParamsDTO pagination)
        {
            var emprestimos = await _emprestimoService.GetPagedAsync(pagination);
            return Ok(emprestimos);
        }

        [HttpGet("all")]
        [Authorize(Roles = $"{Roles.Admin},{Roles.Bibliotecario}")]
        public async Task<IActionResult> GetAllUnpaged()
        {
            var emprestimos = await _emprestimoService.GetAllAsync();
            return Ok(emprestimos);
        }

        [HttpGet("meus")]
        [Authorize(Roles = Roles.Aluno)]
        public async Task<ActionResult<List<EmprestimoResponseDTO>>> GetMeusEmprestimos()
        {
            var alunoIdClaim = User.FindFirst("alunoId")?.Value;
            if (string.IsNullOrEmpty(alunoIdClaim) || !Guid.TryParse(alunoIdClaim, out var alunoId))
            {
                return BadRequest(new { message = "Aluno não identificado no token." });
            }

            var emprestimos = await _emprestimoService.GetByAlunoIdAsync(alunoId);
            return Ok(emprestimos);
        }
    }
}