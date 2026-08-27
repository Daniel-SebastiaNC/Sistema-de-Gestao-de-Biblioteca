using Models;
using DataContext;
using Microsoft.EntityFrameworkCore;

namespace Repository;

public class LivroRepository : ILivroRepository
{
    private readonly BibliotecaContext _contextDb;

    public LivroRepository(BibliotecaContext contextDb)
    {
        _contextDb = contextDb;
    }

    public async Task<Livro> AddLivroAsync(Livro livro)
    {
        await _contextDb.AddAsync(livro);
        await _contextDb.SaveChangesAsync();
        return livro;
    }

    public async Task<Livro?> GetLivroByIdAsync(Guid id)
    {
        return await _contextDb.Livros
        .Include(l => l.Autor)
        .FirstOrDefaultAsync(l => l.Id.Equals(id));
    }

    public async Task<List<Livro>> GetAllLivrosAsync()
    {
        return await _contextDb.Livros
        .Include(l => l.Autor)
        .ToListAsync();
    }

    public async Task<Livro> UpdateLivroAsync(Livro livro)
    {
        _contextDb.Update(livro);
        await _contextDb.SaveChangesAsync();
        return livro;
    }

    public async Task DeleteLivroAsync(Livro livro)
    {
        _contextDb.Remove(livro);
        await _contextDb.SaveChangesAsync();
    }

    public async Task<List<Livro>> GetLivrosByAutorOrTitleAsync(string? titulo, string? autor)
    {
        IQueryable<Livro> query = _contextDb.Livros.Include(l => l.Autor);

        if (!string.IsNullOrWhiteSpace(titulo))
        {
            query = query.Where(l => l.Titulo.Contains(titulo));
        }

        if (!string.IsNullOrWhiteSpace(autor))
        {
            query = query.Where(l => l.Autor != null && l.Autor.Nome.Contains(autor));
        }

        return await query.ToListAsync();
    }

    public async Task<(List<Livro> Items, int TotalCount)> GetPagedLivrosByAutorOrTitleAsync(string? titulo, string? autor, int pageNumber, int pageSize)
    {
        return await GetPagedLivrosAsync(null, titulo, autor, pageNumber, pageSize);
    }

    public async Task<(List<Livro> Items, int TotalCount)> GetPagedLivrosAsync(string? termo, string? titulo, string? autor, int pageNumber, int pageSize)
    {
        IQueryable<Livro> query = _contextDb.Livros.Include(l => l.Autor);

        if (!string.IsNullOrWhiteSpace(termo))
        {
            var termoLower = termo.ToLower();
            query = query.Where(l =>
                l.Titulo.ToLower().Contains(termoLower) ||
                l.ISBN.ToLower().Contains(termoLower) ||
                (l.Autor != null && l.Autor.Nome.ToLower().Contains(termoLower)));
        }

        if (!string.IsNullOrWhiteSpace(titulo))
        {
            query = query.Where(l => l.Titulo.Contains(titulo));
        }

        if (!string.IsNullOrWhiteSpace(autor))
        {
            query = query.Where(l => l.Autor != null && l.Autor.Nome.Contains(autor));
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<bool> HasActiveLoansAsync(Guid livroId)
    {
        return await _contextDb.Emprestimos.AnyAsync(e => e.LivroId == livroId && e.Status == StatusEmprestimo.Ativo);
    }
}