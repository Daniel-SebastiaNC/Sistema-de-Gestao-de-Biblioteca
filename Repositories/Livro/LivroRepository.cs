using Models;
using DataContext;
    
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

    public Livro? GetLivroById(int id)
    {
        return _contextDb.Livros.FirstOrDefault(l => l.Id.Equals(id));
    }

    public List<Livro> GetAllLivros()
    {
        return _contextDb.Livros.ToList();
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
}