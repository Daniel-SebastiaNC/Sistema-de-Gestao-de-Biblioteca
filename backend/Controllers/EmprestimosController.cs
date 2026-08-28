using Microsoft.AspNetCore.Mvc;
using DTO;
using Services;

namespace Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmprestimosController : ControllerBase
    {
        private readonly IEmprestimoService _emprestimoService;

        public EmprestimosController(IEmprestimoService emprestimoService)
        {
            _emprestimoService = emprestimoService;
        }

        [HttpPost]
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
        public async Task<IActionResult> ReturnEmprestimo(Guid id)
        {
            var emprestimoAtualizado = await _emprestimoService.ReturnEmprestimoAsync(id);
            return Ok(emprestimoAtualizado);
        }

        [HttpPost("devolver")]
        public async Task<ActionResult<DevolucaoResponseDTO>> Devolver([FromBody] DevolverEmprestimoDTO dto)
        {
            var devolucao = await _emprestimoService.DevolverComCalculoMultaAsync(dto);
            return Ok(devolucao);
        }

        [HttpGet]
        public async Task<ActionResult<PagedResultDTO<EmprestimoResponseDTO>>> GetAll([FromQuery] PaginationParamsDTO pagination)
        {
            var emprestimos = await _emprestimoService.GetPagedAsync(pagination);
            return Ok(emprestimos);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllUnpaged()
        {
            var emprestimos = await _emprestimoService.GetAllAsync();
            return Ok(emprestimos);
        }
    }
}