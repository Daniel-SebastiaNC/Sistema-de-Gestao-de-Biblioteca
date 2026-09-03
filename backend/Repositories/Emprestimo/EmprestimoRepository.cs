using Models;
using DataContext;
using Microsoft.EntityFrameworkCore;

namespace Repository;

public class EmprestimoRepository : IEmprestimoRepository
{
    private readonly BibliotecaContext _contextDb;

    public EmprestimoRepository(BibliotecaContext contextDb)
    {
        _contextDb = contextDb;
    }

    public async Task<Emprestimo> AddEmprestimoAsync(Emprestimo emprestimo)
    {
        await _contextDb.AddAsync(emprestimo);
        await _contextDb.SaveChangesAsync();
        return emprestimo;
    }

    public async Task<Emprestimo?> GetEmprestimoByIdAsync(Guid id)
    {
        return await _contextDb.Emprestimos
            .Include(e => e.Aluno)
            .Include(e => e.Livro)
                .ThenInclude(l => l.Autor)
            .FirstOrDefaultAsync(e => e.Id.Equals(id));
    }

    public async Task<Emprestimo> UpdateEmprestimoAsync(Emprestimo emprestimo)
    {
        _contextDb.Update(emprestimo);
        await _contextDb.SaveChangesAsync();
        return emprestimo;
    }

    public async Task<List<Emprestimo>> GetAllAsync()
    {
        return await _contextDb.Emprestimos
            .Include(e => e.Aluno)
            .Include(e => e.Livro)
                .ThenInclude(l => l.Autor)
            .ToListAsync();
    }

    public async Task<(List<Emprestimo> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize)
    {
        var query = _contextDb.Emprestimos
            .Include(e => e.Aluno)
            .Include(e => e.Livro)
                .ThenInclude(l => l.Autor);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<bool> ExistsEmpresitimoAtivoAsync(Guid idAluno, Guid idLivro)
    {
        return await _contextDb.Emprestimos.AnyAsync(e =>
        e.AlunoId == idAluno &&
        e.LivroId == idLivro &&
        e.Status == StatusEmprestimo.Ativo);
    }

    public async Task<List<Emprestimo>> GetByAlunoIdAsync(Guid alunoId)
    {
        return await _contextDb.Emprestimos
            .Include(e => e.Aluno)
            .Include(e => e.Livro)
                .ThenInclude(l => l.Autor)
            .Where(e => e.AlunoId == alunoId)
            .OrderByDescending(e => e.DataEmprestimo)
            .ToListAsync();
    }
}