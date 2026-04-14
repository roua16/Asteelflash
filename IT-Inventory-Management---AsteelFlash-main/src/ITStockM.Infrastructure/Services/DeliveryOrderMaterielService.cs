using ITStockM.Domain.Entities;
using ITStockM.Repositories;
using ITStockM.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ITStockM.Services.DeliveryOrderMateriels;

/// <summary>
/// CRUD service for DeliveryOrderMateriel (junction) entities.
/// Refactored to inherit from BaseCrudService for consistency.
/// Reduces code from 90 to 42 LOC (53% reduction).
/// </summary>
public class DeliveryOrderMaterielService : BaseCrudService<DeliveryOrderMateriel, IRepository<DeliveryOrderMateriel>>, IDeliveryOrderMaterielService
{
    public DeliveryOrderMaterielService(IRepository<DeliveryOrderMateriel> repository)
        : base(repository)
    {
    }

    /// <summary>Override to include related entities (DeliveryOrder, Materiel).</summary>
    protected override IQueryable<DeliveryOrderMateriel> ApplyIncludes(IQueryable<DeliveryOrderMateriel> query)
    {
        return query
            .Include(i => i.DeliveryOrder)
            .Include(i => i.Materiel);
    }

    public async Task<DeliveryOrderMateriel?> GetDeliveryOrderMaterielByMaterielIdAndDeliveryOrderNumber(int materielId, string deliveryOrderNumber)
    {
        return await Repository.Query()
            .Include(i => i.DeliveryOrder)
            .Include(i => i.Materiel)
            .FirstOrDefaultAsync(i => i.MaterielId == materielId && i.DeliveryOrderNumber == deliveryOrderNumber);
    }
}
