using Models;
using DataContext;
using Microsoft.EntityFrameworkCore;

namespace Repository;
public class AutorRepository : IAutorRepository
{
    private readonly BibliotecaContext _contextDb;

    public AutorRepository(BibliotecaContext contextDb)
    {
        _contextDb = contextDb;
    }

    public async Task<Autor> AddAutorAsync(Autor autor)
    {
        await _contextDb.AddAsync(autor);
        await _contextDb.SaveChangesAsync();
        return autor;
    }

    public async Task<Autor?> GetAutorByIdAsync(Guid id)
    {
        return await _contextDb.Autores.FirstOrDefaultAsync(a => a.Id.Equals(id));
    }

    public async Task<List<Autor>> GetAllAutoresAsync()
    {
        return await _contextDb.Autores.ToListAsync();
    }

    public async Task<Autor> UpdateAutorAsync(Autor autor)
    {
        _contextDb.Update(autor);
        await _contextDb.SaveChangesAsync();
        return autor;
    }

    public async Task DeleteAutorAsync(Autor autor)
    {
        _contextDb.Remove(autor);
        await _contextDb.SaveChangesAsync();
    }
}