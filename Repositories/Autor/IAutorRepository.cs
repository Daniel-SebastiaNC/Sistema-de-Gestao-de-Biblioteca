using Models;

namespace Repository;
    public interface IAutorRepository
    {
        Autor AddAutor(Autor autor);
        Autor? GetAutorById(Guid id);
        List<Autor> GetAllAutores();
        Autor UpdateAutor(Autor autor);
        void DeleteAutor(Autor autor);
    }