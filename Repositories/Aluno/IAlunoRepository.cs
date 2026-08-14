using Models;

namespace Repository;    
public interface IAlunoRepository
{
    Aluno AddAluno(Aluno aluno);
    Aluno? GetAlunoById(int id);
    List<Aluno> GetAllAlunos();
    Aluno UpdateAluno(Aluno aluno);
    void DeleteAluno(Aluno aluno);
}
