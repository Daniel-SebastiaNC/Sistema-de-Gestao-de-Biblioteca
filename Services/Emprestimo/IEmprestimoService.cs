using DTO;

namespace Services;

public interface IEmprestimoService
{
    Task<EmprestimoResponseDTO> AddEmprestimoAsync(CriarEmprestimoDTO dto);
    Task<EmprestimoResponseDTO> ReturnEmprestimoAsync(Guid id);

    Task<List<EmprestimoResponseDTO>> GetAllAsync();
    Task<PagedResultDTO<EmprestimoResponseDTO>> GetPagedAsync(PaginationParamsDTO paginationParams);
}