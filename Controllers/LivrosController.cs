using Microsoft.AspNetCore.Mvc;
using DTO;
using Services;

namespace Controllers
{
    [ApiController]
    [Route("api/[controller]")] // A rota será /api/livros
    public class LivrosController : ControllerBase
    {
        private readonly ILivroService _livroService;

        public LivrosController(ILivroService livroService)
        {
            _livroService = livroService;
        }

        // POST /api/livros
        [HttpPost]
        public IActionResult CriarLivro([FromBody] CriarLivroDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var livroCriado = _livroService.AddLivro(dto);

            return CreatedAtAction(nameof(GetLivroById), new {id = livroCriado.Id},livroCriado);
        }

        [HttpGet]
        public IActionResult ObterLivros([FromQuery] string? titulo, [FromQuery] string? autor)
        {
            var livros = _livroService.GetLivrosByAutorOrTitle(titulo, autor);
            
            return Ok(livros);
        }

        [HttpGet("{id}")]
        public IActionResult GetLivroById(Guid id)
        {
            var livros = _livroService.GetLivrosById(id);
            
            return Ok(livros);
        }

        [HttpGet("api/[controller]/all")]
        public IActionResult GetAll()
        {
            List<LivroResponseDTO> livros = _livroService.GetAll();
            
            return Ok(livros);
        }

    }
}