using DTO;

namespace Services;

public interface IReservaService
{
    Task<ReservaResponseDTO> AddReservaAsync(CriarReservaDTO dto);
    Task<List<ReservaResponseDTO>> GetFilaEsperaAsync(Guid livroId);
    Task<List<ReservaResponseDTO>> GetByAlunoIdAsync(Guid alunoId);
}
