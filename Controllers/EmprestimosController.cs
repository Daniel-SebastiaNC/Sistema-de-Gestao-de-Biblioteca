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
        public IActionResult AddEmprestimo([FromBody] CriarEmprestimoDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var emprestimo = _emprestimoService.AddEmprestimo(dto);
            return Created($"/api/emprestimos/{emprestimo.Id}", emprestimo);
            
        }

        [HttpPut("{id}/devolucao")]
        public IActionResult ReturnEmprestimo(Guid id)
        {
            var emprestimoAtualizado = _emprestimoService.ReturnEmprestimo(id);
            return Ok(emprestimoAtualizado);
            
        }

        [HttpGet("api/[controller]/all")]
        public IActionResult GetAll()
        {
            return Ok(_emprestimoService.GetAll());
        }
    }
}