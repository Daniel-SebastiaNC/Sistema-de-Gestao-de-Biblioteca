using Models;

namespace Repository;
    public interface IEmprestimoRepository
    {
        Task<Emprestimo> AddEmprestimoAsync(Emprestimo emprestimo);
        Task<Emprestimo?> GetEmprestimoByIdAsync(Guid id);
        Task<Emprestimo> UpdateEmprestimoAsync(Emprestimo emprestimo);
        Task<List<Emprestimo>> GetAllAsync();
        Task<bool> ExistsEmpresitimoAtivoAsync(Guid idAluno, Guid idLivro);
    }