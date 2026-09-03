using Models;

namespace Repository;

public interface IUsuarioRepository
{
    Task<Usuario> AddAsync(Usuario usuario);
    Task<Usuario?> GetByIdAsync(Guid id);
    Task<Usuario?> GetByEmailAsync(string email);
    Task<bool> ExistsByEmailAsync(string email);
    Task<List<Usuario>> GetAllAsync();
    Task UpdateAsync(Usuario usuario);
}
