using Models;

namespace Repository;
    public interface ILivroRepository
    {
        Livro AddLivro(Livro livro);
        Livro? GetLivroById(Guid id);
        List<Livro> GetAllLivros();
        Livro UpdateLivro(Livro livro);
        void DeleteLivro(Livro livro);

        public List<Livro> GetLivrosByAutorOrTitle(string? titulo, string? autor);
    }
