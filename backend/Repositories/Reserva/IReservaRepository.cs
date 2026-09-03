using Models;

namespace Repository;

public interface IReservaRepository
{
    Task<Reserva> AddReservaAsync(Reserva reserva);
    Task<Reserva?> GetReservaByIdAsync(Guid id);
    Task<List<Reserva>> GetFilaEsperaByLivroIdAsync(Guid livroId);
    Task<bool> ExistsReservaAtivaAsync(Guid alunoId, Guid livroId);
    Task<int> CountReservasAtivasAsync();
    Task<Reserva> UpdateReservaAsync(Reserva reserva);
    Task<List<Reserva>> GetByAlunoIdAsync(Guid alunoId);
    Task<List<Reserva>> GetAllReservasAsync();
}
