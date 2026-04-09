using ITStockM.Models.ITStockManagment;
using Radzen;

namespace ITStockM.Services.DeliveryOrderMateriels;

public class DeliveryOrderMaterielService : IDeliveryOrderMaterielService
{
    private readonly ITStockManagmentService stockService;

    public DeliveryOrderMaterielService(ITStockManagmentService stockService)
    {
        this.stockService = stockService;
    }

    public Task<IQueryable<DeliveryOrderMateriel>> GetDeliveryOrderMateriels(Query query = null)
        => stockService.GetDeliveryOrderMateriels(query);

    public Task<DeliveryOrderMateriel?> GetDeliveryOrderMaterielByMaterielIdAndDeliveryOrderNumber(int materielId, string deliveryOrderNumber)
        => stockService.GetDeliveryOrderMaterielByMaterielIdAndDeliveryOrderNumber(materielId, deliveryOrderNumber);

    public Task<DeliveryOrderMateriel> CreateDeliveryOrderMateriel(DeliveryOrderMateriel deliveryOrderMateriel)
        => stockService.CreateDeliveryOrderMateriel(deliveryOrderMateriel);

    public Task<DeliveryOrderMateriel> UpdateDeliveryOrderMateriel(int materielId, string deliveryOrderNumber, DeliveryOrderMateriel deliveryOrderMateriel)
        => stockService.UpdateDeliveryOrderMateriel(materielId, deliveryOrderNumber, deliveryOrderMateriel);

    public Task<DeliveryOrderMateriel> DeleteDeliveryOrderMateriel(int materielId, string deliveryOrderNumber)
        => stockService.DeleteDeliveryOrderMateriel(materielId, deliveryOrderNumber);
}
