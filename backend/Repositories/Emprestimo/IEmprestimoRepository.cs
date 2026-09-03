using Models;

namespace Repository;

public interface IEmprestimoRepository
{
    Task<Emprestimo> AddEmprestimoAsync(Emprestimo emprestimo);
    Task<Emprestimo?> GetEmprestimoByIdAsync(Guid id);
    Task<Emprestimo> UpdateEmprestimoAsync(Emprestimo emprestimo);
    Task<List<Emprestimo>> GetAllAsync();
    Task<(List<Emprestimo> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize);
    Task<bool> ExistsEmpresitimoAtivoAsync(Guid idAluno, Guid idLivro);
    Task<List<Emprestimo>> GetByAlunoIdAsync(Guid alunoId);
}