using Models;

namespace Repository;    
public interface IAlunoRepository
{
    Aluno AddAluno(Aluno aluno);
    Aluno? GetAlunoById(Guid id);
    List<Aluno> GetAllAlunos();
    Aluno UpdateAluno(Aluno aluno);
    void DeleteAluno(Aluno aluno);

    bool ExistsAlunoByMatricula(string matricula);
}
