using Models;
using DataContext;

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
            _contextDb.Emprestimos.Add(emprestimo);
            _contextDb.SaveChanges();
            return emprestimo;
        }

        public void DeleteEmprestimo(Emprestimo emprestimo)
        {
            _contextDb.Emprestimos.Remove(emprestimo);
            _contextDb.SaveChanges();
        }

        public List<Emprestimo> GetAllEmprestimos()
        {
            return _contextDb.Emprestimos.ToList();
        }

        public Emprestimo? GetEmprestimoById(int id)
        {
            return _contextDb.Emprestimos.FirstOrDefault(e => e.Id.Equals(id));
        }

        public Emprestimo UpdateEmprestimo(Emprestimo emprestimo)
        {
            _contextDb.Emprestimos.Update(emprestimo);
            _contextDb.SaveChanges();
            return emprestimo;
        }
    }