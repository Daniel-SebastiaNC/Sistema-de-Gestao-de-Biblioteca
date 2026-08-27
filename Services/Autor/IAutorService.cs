using DTO;

namespace Services;

public interface IAutorService
{
    Task<AutorResponseDto> AddAutorAsync(CriarAutorDto dto);
    Task<AutorResponseDto> GetAutorByIdAsync(Guid id);
    Task<List<AutorResponseDto>> GetAllAutoresAsync();
    Task<PagedResultDTO<AutorResponseDto>> GetPagedAutoresAsync(PaginationParamsDTO paginationParams);
    Task<AutorResponseDto> UpdateAutorAsync(Guid id, CriarAutorDto dto);
    Task DeleteAutorAsync(Guid id);
}