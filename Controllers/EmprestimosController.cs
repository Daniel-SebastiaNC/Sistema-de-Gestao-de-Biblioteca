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

            try
            {
                var emprestimo = _emprestimoService.AddEmprestimo(dto);
                return Created($"/api/emprestimos/{emprestimo.Id}", emprestimo);
            }
            catch (Exception ex)
            {
                // Se faltar estoque ou livro não existir, o throw do Service cai aqui
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        [HttpPut("{id}/devolucao")]
        public IActionResult ReturnEmprestimo(Guid id)
        {
            try
            {
                var emprestimoAtualizado = _emprestimoService.ReturnEmprestimo(id);
                return Ok(emprestimoAtualizado);
            }
            catch (Exception ex)
            {
                // Se o empréstimo não for encontrado ou já estiver devolvido, cai aqui
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        [HttpGet("api/[controller]/all")]
        public IActionResult GetAll()
        {
            return Ok(_emprestimoService.GetAll());
        }
    }
}