using Models;

namespace Repository;
    public interface IAutorRepository
    {
        Task<Autor> AddAutorAsync(Autor autor);
        Task<Autor?> GetAutorByIdAsync(Guid id);
        Task<List<Autor>> GetAllAutoresAsync();
        Task<Autor> UpdateAutorAsync(Autor autor);
        Task DeleteAutorAsync(Autor autor);
    }