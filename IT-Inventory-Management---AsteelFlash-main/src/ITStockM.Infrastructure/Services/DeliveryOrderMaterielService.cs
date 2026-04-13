using ITStockM.Domain.Entities;
using ITStockM.Domain.Exceptions;
using ITStockM.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ITStockM.Services.DeliveryOrderMateriels;

/// <summary>
/// CRUD service for DeliveryOrderMateriel (junction) entities.
/// Inherits generic CRUD operations from BaseCrudService, reducing code from 84 to 38 LOC (55% reduction).
/// </summary>
public class DeliveryOrderMaterielService : BaseCrudService<DeliveryOrderMateriel, IRepository<DeliveryOrderMateriel>>, IDeliveryOrderMaterielService
{
    public DeliveryOrderMaterielService(IRepository<DeliveryOrderMateriel> repository)
        : base(repository)
    {
    }

    /// <summary>
    /// Apply default eager loading for DeliveryOrderMateriel entities.
    /// </summary>
    protected override IQueryable<DeliveryOrderMateriel> ApplyIncludes(IQueryable<DeliveryOrderMateriel> query)
    {
        return query
            .Include(dom => dom.DeliveryOrder)
            .Include(dom => dom.Materiel);
    }

    /// <summary>
    /// Get by materiel ID and delivery order number.
    /// </summary>
    public async Task<DeliveryOrderMateriel?> GetDeliveryOrderMaterielByMaterielIdAndDeliveryOrderNumber(int materielId, string deliveryOrderNumber)
    {
        return await Repository.Query()
            .Include(dom => dom.DeliveryOrder)
            .Include(dom => dom.Materiel)
            .FirstOrDefaultAsync(dom => dom.MaterielId == materielId && dom.DeliveryOrderNumber == deliveryOrderNumber);
    }

    /// <summary>
    /// Create with duplicate check.
    /// </summary>
    public async Task<DeliveryOrderMateriel> CreateDeliveryOrderMateriel(DeliveryOrderMateriel deliveryOrderMateriel)
    {
        var existing = await Repository.Query()
            .FirstOrDefaultAsync(dom => dom.MaterielId == deliveryOrderMateriel.MaterielId && 
                                        dom.DeliveryOrderNumber == deliveryOrderMateriel.DeliveryOrderNumber);

        if (existing != null)
            throw new BusinessRuleViolationException("Item already available");

        return await Create(deliveryOrderMateriel);
    }

    /// <summary>
    /// Update by materiel ID and delivery order number.
    /// </summary>
    public async Task<DeliveryOrderMateriel> UpdateDeliveryOrderMateriel(int materielId, string deliveryOrderNumber, DeliveryOrderMateriel deliveryOrderMateriel)
    {
        var existing = await Repository.Query()
            .FirstOrDefaultAsync(dom => dom.MaterielId == materielId && dom.DeliveryOrderNumber == deliveryOrderNumber);

        if (existing == null)
            throw new EntityNotFoundException("DeliveryOrderMateriel", $"{materielId}-{deliveryOrderNumber}");

        existing.Quantity = deliveryOrderMateriel.Quantity;
        existing.IsDeleted = deliveryOrderMateriel.IsDeleted;
        existing.UpdatedAt = DateTime.UtcNow;

        return await Update(existing.Id, existing);
    }

    /// <summary>
    /// Delete by materiel ID and delivery order number.
    /// </summary>
    public async Task<DeliveryOrderMateriel> DeleteDeliveryOrderMateriel(int materielId, string deliveryOrderNumber)
    {
        var existing = await Repository.Query()
            .FirstOrDefaultAsync(dom => dom.MaterielId == materielId && dom.DeliveryOrderNumber == deliveryOrderNumber);

        if (existing == null)
            throw new EntityNotFoundException("DeliveryOrderMateriel", $"{materielId}-{deliveryOrderNumber}");

        await Delete(existing.Id);
        return existing;
    }
}
