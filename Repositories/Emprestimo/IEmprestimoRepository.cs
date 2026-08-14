using Models;

namespace Repository;
    public interface IEmprestimoRepository
    {
        Emprestimo AddEmprestimo(Emprestimo emprestimo);
        Emprestimo? GetEmprestimoById(int id);
        List<Emprestimo> GetAllEmprestimos();
        Emprestimo UpdateEmprestimo(Emprestimo emprestimo);
        void DeleteEmprestimo(Emprestimo emprestimo);
    }