using DTO;

namespace Services;

public interface IAlunoService
{
    Task<AlunoResponseDTO> AddAlunoAsync(CriarAlunoDTO dto);
    Task<AlunoResponseDTO> GetAlunoByIdAsync(Guid id);
    Task<List<AlunoResponseDTO>> GetAllAlunosAsync();
    Task<PagedResultDTO<AlunoResponseDTO>> GetPagedAlunosAsync(PaginationParamsDTO paginationParams);
    Task DeleteAlunoAsync(Guid id);
}