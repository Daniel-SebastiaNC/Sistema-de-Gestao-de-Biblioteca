using DTO;

namespace Services;
public interface IAlunoService
{
    AlunoResponseDTO AddAluno(CriarAlunoDTO dto);
    AlunoResponseDTO GetAlunoById(Guid id);
    List<AlunoResponseDTO> GetAllAlunos();
    void DeleteAluno(Guid id);
}