using Microsoft.EntityFrameworkCore;
using ITStockM.Data;
using ITStockM.Domain.Entities;

namespace ITStockM.Repositories;

public class DeliveryOrderRepository : EfRepository<DeliveryOrder>, IDeliveryOrderRepository
{
    public DeliveryOrderRepository(ITStockManagmentContext context) : base(context)
    {
    }

    public IQueryable<DeliveryOrder> QueryWithIncludes()
    {
        return _dbSet.Include(d => d.Supplier)
                     .Include(d => d.Employee)
                     .Include(d => d.DeliveryOrderMateriels).ThenInclude(dom => dom.Materiel)
                     .AsQueryable();
    }

    public async Task<DeliveryOrder?> GetByNumberWithRelatedAsync(string deleveryOrderNumber, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(d => d.Supplier)
            .Include(d => d.Employee)
            .Include(d => d.DeliveryOrderMateriels).ThenInclude(dom => dom.Materiel)
            .FirstOrDefaultAsync(d => d.DeleveryOrderNumber == deleveryOrderNumber, cancellationToken);
    }
}
