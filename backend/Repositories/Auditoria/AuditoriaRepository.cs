using DataContext;
using Microsoft.EntityFrameworkCore;
using Models;

namespace Repository;

public class AuditoriaRepository : IAuditoriaRepository
{
    private readonly BibliotecaContext _contextDb;

    public AuditoriaRepository(BibliotecaContext contextDb)
    {
        _contextDb = contextDb;
    }

    public async Task RegistrarAsync(Auditoria auditoria)
    {
        await _contextDb.Auditorias.AddAsync(auditoria);
        await _contextDb.SaveChangesAsync();
    }

    public async Task<(List<Auditoria> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize)
    {
        var query = _contextDb.Auditorias.OrderByDescending(a => a.DataHora);
        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }
}
