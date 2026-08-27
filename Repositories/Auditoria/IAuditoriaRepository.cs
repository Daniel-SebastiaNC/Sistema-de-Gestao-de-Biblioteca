using Models;

namespace Repository;

public interface IAuditoriaRepository
{
    Task RegistrarAsync(Auditoria auditoria);
    Task<(List<Auditoria> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize);
}
