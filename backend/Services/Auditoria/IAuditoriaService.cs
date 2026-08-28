using DTO;

namespace Services;

public interface IAuditoriaService
{
    Task RegistrarAcaoAsync(string acao, string detalhes, string usuario = "Sistema");
    Task<PagedResultDTO<AuditoriaResponseDTO>> GetPagedAsync(PaginationParamsDTO paginationParams);
}
