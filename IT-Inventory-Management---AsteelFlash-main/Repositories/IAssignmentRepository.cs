using ITStockM.Models.ITStockManagment;

namespace ITStockM.Repositories;

public interface IAssignmentRepository : IRepository<Assignment>
{
    IQueryable<Assignment> QueryWithIncludes();
    Task<Assignment?> GetByIdWithRelatedAsync(int id, CancellationToken cancellationToken = default);
}
