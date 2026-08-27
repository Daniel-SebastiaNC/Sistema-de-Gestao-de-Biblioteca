using DTO;
namespace Services;

public interface ILivroService
{
    Task<LivroResponseDTO> AddLivroAsync(CriarLivroDto dto);
    Task<List<LivroResponseDTO>> GetLivrosByAutorOrTitleAsync(string? titulo, string? autor);
    Task<PagedResultDTO<LivroResponseDTO>> GetPagedLivrosAsync(string? titulo, string? autor, PaginationParamsDTO paginationParams);
    Task<LivroResponseDTO> GetLivrosByIdAsync(Guid id);

    Task<List<LivroResponseDTO>> GetAllAsync();
}

