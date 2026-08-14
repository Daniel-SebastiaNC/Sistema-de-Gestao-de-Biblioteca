using Models;
using DataContext;

namespace Repository;
public class AlunoRepository : IAlunoRepository
{
    private readonly BibliotecaContext _contextDb;

    public AlunoRepository(BibliotecaContext contextDb)
    {
        _contextDb = contextDb;
    }

    public Aluno AddAluno(Aluno aluno)
    {
        _contextDb.Add(aluno);
        _contextDb.SaveChanges();
        return aluno;
    }

    public Aluno? GetAlunoById(int id)
    {
        return _contextDb.Alunos.FirstOrDefault(a => a.Id.Equals(id));
    }

    public List<Aluno> GetAllAlunos()
    {
        return _contextDb.Alunos.ToList();
    }

    public Aluno UpdateAluno(Aluno aluno)
    {
        _contextDb.Update(aluno);
        _contextDb.SaveChanges();
        return aluno;
    }

    public void DeleteAluno(Aluno aluno)
    {
        _contextDb.Remove(aluno);
        _contextDb.SaveChanges();
    }
}