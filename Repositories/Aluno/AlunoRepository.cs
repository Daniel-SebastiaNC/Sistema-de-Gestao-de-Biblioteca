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

    public async Task<Aluno> UpdateAlunoAsync(Aluno aluno)
    {
        _contextDb.Update(aluno);
        await _contextDb.SaveChangesAsync();
        return aluno;
    }

    public async Task DeleteAlunoAsync(Aluno aluno)
    {
        _contextDb.Remove(aluno);
        await _contextDb.SaveChangesAsync();
    }

    public async Task<bool> ExistsAlunoByMatriculaAsync(string matricula)
    {
        return await _contextDb.Alunos.AnyAsync(a => a.Matricula == matricula);
    }
}