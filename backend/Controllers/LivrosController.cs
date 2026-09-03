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
    public class LivrosController : ControllerBase
    {
        private readonly ILivroService _livroService;

        public LivrosController(ILivroService livroService)
        {
            _livroService = livroService;
        }

        [HttpPost]
        [Authorize(Roles = $"{Roles.Admin},{Roles.Bibliotecario}")]
        public async Task<IActionResult> CriarLivro(CriarLivroDto dto)
        {

            var livroCriado = await _livroService.AddLivroAsync(dto);

            return CreatedAtAction(nameof(GetLivroById), new { id = livroCriado.Id }, livroCriado);
        }

        [HttpGet]
        public async Task<ActionResult<PagedResultDTO<LivroResponseDTO>>> ObterLivros(
            [FromQuery] string? termo,
            [FromQuery] string? titulo,
            [FromQuery] string? autor,
            [FromQuery] PaginationParamsDTO pagination)
        {
            var livros = await _livroService.GetPagedLivrosAsync(termo, titulo, autor, pagination);

            return Ok(livros);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetLivroById(Guid id)
        {
            var livros = await _livroService.GetLivrosByIdAsync(id);

            return Ok(livros);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = $"{Roles.Admin},{Roles.Bibliotecario}")]
        public async Task<ActionResult<LivroResponseDTO>> AtualizarLivro(Guid id, [FromBody] AtualizarLivroDto dto)
        {
            var livroAtualizado = await _livroService.UpdateLivroAsync(id, dto);
            return Ok(livroAtualizado);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = $"{Roles.Admin},{Roles.Bibliotecario}")]
        public async Task<IActionResult> DeleteLivro(Guid id)
        {
            await _livroService.DeleteLivroAsync(id);
            return NoContent();
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            List<LivroResponseDTO> livros = await _livroService.GetAllAsync();

            return Ok(livros);
        }
    }
}