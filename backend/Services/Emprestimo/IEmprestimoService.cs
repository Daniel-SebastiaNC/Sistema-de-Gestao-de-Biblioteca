using DTO;

namespace Services;

public interface IEmprestimoService
{
    Task<EmprestimoResponseDTO> AddEmprestimoAsync(CriarEmprestimoDTO dto);
    Task<EmprestimoResponseDTO> ReturnEmprestimoAsync(Guid id);
    Task<DevolucaoResponseDTO> DevolverComCalculoMultaAsync(DevolverEmprestimoDTO dto);

    Task<List<EmprestimoResponseDTO>> GetAllAsync();
    Task<PagedResultDTO<EmprestimoResponseDTO>> GetPagedAsync(PaginationParamsDTO paginationParams);
    Task<List<EmprestimoResponseDTO>> GetByAlunoIdAsync(Guid alunoId);
}