using Microsoft.EntityFrameworkCore;
using ITStockM.Data;
using ITStockM.Models.ITStockManagment;

namespace ITStockM.Repositories;

public class RequestRepository : EfRepository<Request>, IRequestRepository
{
    public RequestRepository(ITStockManagmentContext context) : base(context)
    {
    }

    public IQueryable<Request> QueryWithIncludes()
    {
        return _dbSet.Include(r => r.Employee)
                     .Include(r => r.Offers)
                     .AsQueryable();
    }

    public async Task<Request?> GetByIdWithRelatedAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(r => r.Employee)
            .Include(r => r.Offers)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }
}
