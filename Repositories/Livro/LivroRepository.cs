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

    public Livro AddLivro(Livro livro)
    {
        _contextDb.Add(livro);
        _contextDb.SaveChanges();
        return livro;
    }

    public Livro? GetLivroById(Guid id)
    {
        return _contextDb.Livros
        .Include(l => l.Autor)
        .FirstOrDefault(l => l.Id.Equals(id));
    }

    public List<Livro> GetAllLivros()
    {
        return _contextDb.Livros
        .Include(l => l.Autor)
        .ToList();
    }

    public Livro UpdateLivro(Livro livro)
    {
        _contextDb.Update(livro);
        _contextDb.SaveChanges();
        return livro;
    }

    public void DeleteLivro(Livro livro)
    {
        _contextDb.Remove(livro);
        _contextDb.SaveChanges();
    }

    public List<Livro> GetLivrosByAutorOrTitle(string? titulo, string? autor)
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

        return query.ToList();
    }
}