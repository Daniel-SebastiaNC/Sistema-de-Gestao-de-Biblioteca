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

        [HttpGet("api/[controller]/all")]
        public async Task<IActionResult> GetAll()
        {
            var emprestimos = await _emprestimoService.GetAllAsync();
            return Ok(emprestimos);
        }
    }
}