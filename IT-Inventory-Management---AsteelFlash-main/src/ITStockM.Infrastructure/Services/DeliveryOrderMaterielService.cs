using ITStockM.Domain.Entities;
using ITStockM.Data;
using ITStockM.Services.Utilities;
using Microsoft.EntityFrameworkCore;
using Radzen;

namespace ITStockM.Services.DeliveryOrderMateriels;

public class DeliveryOrderMaterielService : IDeliveryOrderMaterielService
{
    private readonly ITStockManagmentContext _context;

    public DeliveryOrderMaterielService(ITStockManagmentContext context)
    {
        _context = context;
    }

    public async Task<IQueryable<DeliveryOrderMateriel>> GetDeliveryOrderMateriels(Query query = null)
    {
        IQueryable<DeliveryOrderMateriel> items = _context.DeliveryOrderMateriels
            .Include(i => i.DeliveryOrder)
            .Include(i => i.Materiel);

        if (query != null)
        {
            items = items.ApplyQuery(query);
        }

        return await Task.FromResult(items);
    }

    public async Task<DeliveryOrderMateriel?> GetDeliveryOrderMaterielByMaterielIdAndDeliveryOrderNumber(int materielId, string deliveryOrderNumber)
    {
        return await _context.DeliveryOrderMateriels
            .Include(i => i.DeliveryOrder)
            .Include(i => i.Materiel)
            .FirstOrDefaultAsync(i => i.MaterielId == materielId && i.DeliveryOrderNumber == deliveryOrderNumber);
    }

    public async Task<DeliveryOrderMateriel> CreateDeliveryOrderMateriel(DeliveryOrderMateriel deliveryOrderMateriel)
    {
        var existing = await _context.DeliveryOrderMateriels
            .FirstOrDefaultAsync(i => i.MaterielId == deliveryOrderMateriel.MaterielId && i.DeliveryOrderNumber == deliveryOrderMateriel.DeliveryOrderNumber);

        if (existing != null)
        {
            throw new InvalidOperationException("Item already available");
        }

        _context.DeliveryOrderMateriels.Add(deliveryOrderMateriel);
        await _context.SaveChangesAsync();
        return deliveryOrderMateriel;
    }

    public async Task<DeliveryOrderMateriel> UpdateDeliveryOrderMateriel(int materielId, string deliveryOrderNumber, DeliveryOrderMateriel deliveryOrderMateriel)
    {
        var existing = await _context.DeliveryOrderMateriels
            .FirstOrDefaultAsync(i => i.MaterielId == materielId && i.DeliveryOrderNumber == deliveryOrderNumber);

        if (existing is null)
        {
            throw new KeyNotFoundException("Item no longer available");
        }

        _context.Entry(existing).CurrentValues.SetValues(deliveryOrderMateriel);
        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<DeliveryOrderMateriel> DeleteDeliveryOrderMateriel(int materielId, string deliveryOrderNumber)
    {
        var existing = await _context.DeliveryOrderMateriels
            .FirstOrDefaultAsync(i => i.MaterielId == materielId && i.DeliveryOrderNumber == deliveryOrderNumber);

        if (existing is null)
        {
            throw new KeyNotFoundException("Item no longer available");
        }

        _context.DeliveryOrderMateriels.Remove(existing);
        await _context.SaveChangesAsync();
        return existing;
    }
}
