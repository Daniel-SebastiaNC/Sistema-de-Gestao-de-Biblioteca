using Models;

namespace Repository;    
public interface IAlunoRepository
{
    Task<Aluno> AddAlunoAsync(Aluno aluno);
    Task<Aluno?> GetAlunoByIdAsync(Guid id);
    Task<List<Aluno>> GetAllAlunosAsync();
    Task<Aluno> UpdateAlunoAsync(Aluno aluno);
    Task DeleteAlunoAsync(Aluno aluno);

    Task<bool> ExistsAlunoByMatriculaAsync(string matricula);
}

