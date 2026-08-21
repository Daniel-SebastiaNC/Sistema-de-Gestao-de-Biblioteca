using Models;

namespace Repository;
    public interface ILivroRepository
    {
        Task<Livro> AddLivroAsync(Livro livro);
        Task<Livro?> GetLivroByIdAsync(Guid id);
        Task<List<Livro>> GetAllLivrosAsync();
        Task<Livro> UpdateLivroAsync(Livro livro);
        Task DeleteLivroAsync(Livro livro);

        Task<List<Livro>> GetLivrosByAutorOrTitleAsync(string? titulo, string? autor);
    }

