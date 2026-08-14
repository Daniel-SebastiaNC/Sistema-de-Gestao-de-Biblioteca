using Models;

namespace Repository;
    public interface IAutorRepository
    {
        Autor AddAutor(Autor autor);
        Autor? GetAutorById(int id);
        List<Autor> GetAllAutores();
        Autor UpdateAutor(Autor autor);
        void DeleteAutor(Autor autor);
    }