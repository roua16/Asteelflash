using ITStockM.Domain.Entities;

namespace ITStockM.Repositories;

public interface IRequestRepository : IRepository<Request>
{
    IQueryable<Request> QueryWithIncludes();
    Task<Request?> GetByIdWithRelatedAsync(int id, CancellationToken cancellationToken = default);
}
