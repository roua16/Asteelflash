using ITStockM.Application.Common.Models;
using ITStockM.Domain.Entities;

namespace ITStockM.Services.DeliveryOrderMateriels;

public interface IDeliveryOrderMaterielService
{
    Task<IQueryable<DeliveryOrderMateriel>> GetDeliveryOrderMateriels(QueryOptions? query = null);
    Task<DeliveryOrderMateriel?> GetDeliveryOrderMaterielByMaterielIdAndDeliveryOrderNumber(int materielId, string deliveryOrderNumber);
    Task<DeliveryOrderMateriel> CreateDeliveryOrderMateriel(DeliveryOrderMateriel deliveryOrderMateriel);
    Task<DeliveryOrderMateriel> UpdateDeliveryOrderMateriel(int materielId, string deliveryOrderNumber, DeliveryOrderMateriel deliveryOrderMateriel);
    Task<DeliveryOrderMateriel> DeleteDeliveryOrderMateriel(int materielId, string deliveryOrderNumber);
}
