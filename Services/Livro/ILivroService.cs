using DTO;
namespace Services;

public interface ILivroService
{
    LivroResponseDTO AddLivro(CriarLivroDto dto);
    List<LivroResponseDTO> GetLivrosByAutorOrTitle(string? titulo, string? autor);
    LivroResponseDTO GetLivrosById(Guid id);

    List<LivroResponseDTO> GetAll();
}
