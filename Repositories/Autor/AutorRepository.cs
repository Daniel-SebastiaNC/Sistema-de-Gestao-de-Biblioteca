using Models;
using DataContext;

namespace Repository;
public class AutorRepository : IAutorRepository
{
    private readonly BibliotecaContext _contextDb;

    public AutorRepository(BibliotecaContext contextDb)
    {
        _contextDb = contextDb;
    }

    public Autor AddAutor(Autor autor)
    {
        _contextDb.Add(autor);
        _contextDb.SaveChanges();
        return autor;
    }

    public Autor? GetAutorById(Guid id)
    {
        return _contextDb.Autores.FirstOrDefault(a => a.Id.Equals(id));
    }

    public List<Autor> GetAllAutores()
    {
        return _contextDb.Autores.ToList();
    }

    public Autor UpdateAutor(Autor autor)
    {
        _contextDb.Update(autor);
        _contextDb.SaveChanges();
        return autor;
    }

    public void DeleteAutor(Autor autor)
    {
        _contextDb.Remove(autor);
        _contextDb.SaveChanges();
    }
}