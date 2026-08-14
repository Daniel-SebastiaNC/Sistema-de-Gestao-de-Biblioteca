using Models;

namespace Repository;
    public interface IEmprestimoRepository
    {
        Emprestimo AddEmprestimo(Emprestimo emprestimo);
        Emprestimo? GetEmprestimoById(Guid id);
        Emprestimo UpdateEmprestimo(Emprestimo emprestimo);
        List<Emprestimo> GetAll();
    }