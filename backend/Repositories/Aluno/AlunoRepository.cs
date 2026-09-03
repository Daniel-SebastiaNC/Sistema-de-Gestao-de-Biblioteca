using Models;
using DataContext;
using Microsoft.EntityFrameworkCore;

namespace Repository;

public class AlunoRepository : IAlunoRepository
{
    private readonly BibliotecaContext _contextDb;

    public AlunoRepository(BibliotecaContext contextDb)
    {
        _contextDb = contextDb;
    }

    public async Task<Aluno> AddAlunoAsync(Aluno aluno)
    {
        await _contextDb.AddAsync(aluno);
        await _contextDb.SaveChangesAsync();
        return aluno;
    }

    public async Task<Aluno?> GetAlunoByIdAsync(Guid id)
    {
        return await _contextDb.Alunos
        .Include(a => a.Emprestimos)
            .ThenInclude(e => e.Livro)
                .ThenInclude(l => l.Autor)
        .FirstOrDefaultAsync(a => a.Id.Equals(id));
    }

    public async Task<List<Aluno>> GetAllAlunosAsync()
    {
        return await _contextDb.Alunos
        .Include(a => a.Emprestimos)
            .ThenInclude(e => e.Livro)
                .ThenInclude(l => l.Autor)
        .ToListAsync();
    }

    public async Task<(List<Aluno> Items, int TotalCount)> GetPagedAlunosAsync(int pageNumber, int pageSize)
    {
        var query = _contextDb.Alunos
            .Include(a => a.Emprestimos)
                .ThenInclude(e => e.Livro)
                    .ThenInclude(l => l.Autor);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<Aluno> UpdateAlunoAsync(Aluno aluno)
    {
        _contextDb.Update(aluno);
        await _contextDb.SaveChangesAsync();
        return aluno;
    }

    public async Task DeleteAlunoAsync(Aluno aluno)
    {
        if (aluno.UsuarioId.HasValue)
        {
            var usuario = await _contextDb.Usuarios.FindAsync(aluno.UsuarioId.Value);
            if (usuario != null)
            {
                _contextDb.Usuarios.Remove(usuario);
            }
        }
        _contextDb.Remove(aluno);
        await _contextDb.SaveChangesAsync();
    }

    public async Task<bool> ExistsAlunoByMatriculaAsync(string matricula)
    {
        return await _contextDb.Alunos.AnyAsync(a => a.Matricula == matricula);
    }

    public async Task<bool> ExistsAlunoByEmailAsync(string email)
    {
        var emailLower = email.Trim().ToLower();
        return await _contextDb.Alunos.AnyAsync(a => a.Email.ToLower() == emailLower);
    }

    public async Task<Aluno?> GetByUsuarioIdAsync(Guid usuarioId)
    {
        return await _contextDb.Alunos
            .Include(a => a.Emprestimos)
                .ThenInclude(e => e.Livro)
                    .ThenInclude(l => l.Autor)
            .FirstOrDefaultAsync(a => a.UsuarioId == usuarioId);
    }
}