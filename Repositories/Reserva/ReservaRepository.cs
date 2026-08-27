using DataContext;
using Microsoft.EntityFrameworkCore;
using Models;

namespace Repository;

public class ReservaRepository : IReservaRepository
{
    private readonly BibliotecaContext _contextDb;

    public ReservaRepository(BibliotecaContext contextDb)
    {
        _contextDb = contextDb;
    }

    public async Task<Reserva> AddReservaAsync(Reserva reserva)
    {
        await _contextDb.Reservas.AddAsync(reserva);
        await _contextDb.SaveChangesAsync();
        return reserva;
    }

    public async Task<Reserva?> GetReservaByIdAsync(Guid id)
    {
        return await _contextDb.Reservas
            .Include(r => r.Aluno)
            .Include(r => r.Livro)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<List<Reserva>> GetFilaEsperaByLivroIdAsync(Guid livroId)
    {
        return await _contextDb.Reservas
            .Include(r => r.Aluno)
            .Include(r => r.Livro)
            .Where(r => r.LivroId == livroId && r.Status == StatusReserva.Ativa)
            .OrderBy(r => r.DataReserva)
            .ToListAsync();
    }

    public async Task<bool> ExistsReservaAtivaAsync(Guid alunoId, Guid livroId)
    {
        return await _contextDb.Reservas.AnyAsync(r =>
            r.AlunoId == alunoId &&
            r.LivroId == livroId &&
            r.Status == StatusReserva.Ativa);
    }

    public async Task<int> CountReservasAtivasAsync()
    {
        return await _contextDb.Reservas.CountAsync(r => r.Status == StatusReserva.Ativa);
    }

    public async Task<Reserva> UpdateReservaAsync(Reserva reserva)
    {
        _contextDb.Reservas.Update(reserva);
        await _contextDb.SaveChangesAsync();
        return reserva;
    }
}
