using Models;

namespace Repository;
    public interface ILivroRepository
    {
        Livro AddLivro(Livro livro);
        Livro? GetLivroById(int id);
        List<Livro> GetAllLivros();
        Livro UpdateLivro(Livro livro);
        void DeleteLivro(Livro livro);
    }
