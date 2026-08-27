using Microsoft.AspNetCore.Mvc;
using DTO;
using Services;

namespace Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LivrosController : ControllerBase
    {
        private readonly ILivroService _livroService;

        public LivrosController(ILivroService livroService)
        {
            _livroService = livroService;
        }

        [HttpPost]
        public async Task<IActionResult> CriarLivro(CriarLivroDto dto)
        {

            var livroCriado = await _livroService.AddLivroAsync(dto);

            return CreatedAtAction(nameof(GetLivroById), new { id = livroCriado.Id }, livroCriado);
        }

        [HttpGet]
        public async Task<ActionResult<PagedResultDTO<LivroResponseDTO>>> ObterLivros(
            [FromQuery] string? titulo,
            [FromQuery] string? autor,
            [FromQuery] PaginationParamsDTO pagination)
        {
            var livros = await _livroService.GetPagedLivrosAsync(titulo, autor, pagination);

            return Ok(livros);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetLivroById(Guid id)
        {
            var livros = await _livroService.GetLivrosByIdAsync(id);

            return Ok(livros);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            List<LivroResponseDTO> livros = await _livroService.GetAllAsync();

            return Ok(livros);
        }

    }
}