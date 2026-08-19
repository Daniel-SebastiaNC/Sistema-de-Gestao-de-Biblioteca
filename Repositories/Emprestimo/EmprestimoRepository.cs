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

    public Emprestimo AddEmprestimo(Emprestimo emprestimo)
    {
        _contextDb.Add(emprestimo);
        _contextDb.SaveChanges();
        return emprestimo;
    }

    public Emprestimo? GetEmprestimoById(Guid id)
    {
        return _contextDb.Emprestimos
            .Include(e => e.Aluno)
            .Include(e => e.Livro)
                .ThenInclude(l => l.Autor) 
            .FirstOrDefault(e => e.Id.Equals(id));
    }

    public Emprestimo UpdateEmprestimo(Emprestimo emprestimo)
    {
        _contextDb.Update(emprestimo);
        _contextDb.SaveChanges();
        return emprestimo;
    }

    public List<Emprestimo> GetAll()
    {
        return _contextDb.Emprestimos
            .Include(e => e.Aluno)
            .Include(e => e.Livro)
                .ThenInclude(l => l.Autor) 
            .ToList();
    }

    public bool ExistsEmpresitimoAtivo(Guid idAluno, Guid idLivro)
    {
        return _contextDb.Emprestimos.Any(e => 
        e.AlunoId == idAluno && 
        e.LivroId == idLivro && 
        e.Status == StatusEmprestimo.Ativo);
    }
}