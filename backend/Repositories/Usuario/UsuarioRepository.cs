using DataContext;
using Microsoft.EntityFrameworkCore;
using Models;

namespace Repository;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly BibliotecaContext _context;

    public UsuarioRepository(BibliotecaContext context)
    {
        _context = context;
    }

    public async Task<Usuario> AddAsync(Usuario usuario)
    {
        await _context.Usuarios.AddAsync(usuario);
        await _context.SaveChangesAsync();
        return usuario;
    }

    public async Task<Usuario?> GetByIdAsync(Guid id)
    {
        return await _context.Usuarios
            .Include(u => u.Aluno)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<Usuario?> GetByEmailAsync(string email)
    {
        var emailLower = email.Trim().ToLower();
        return await _context.Usuarios
            .Include(u => u.Aluno)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == emailLower);
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        var emailLower = email.Trim().ToLower();
        return await _context.Usuarios.AnyAsync(u => u.Email.ToLower() == emailLower);
    }

    public async Task<List<Usuario>> GetAllAsync()
    {
        return await _context.Usuarios
            .Include(u => u.Aluno)
            .ToListAsync();
    }

    public async Task UpdateAsync(Usuario usuario)
    {
        _context.Usuarios.Update(usuario);
        await _context.SaveChangesAsync();
    }
}
