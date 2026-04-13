using System.Linq;
using ITStockM.Domain.Entities;
using Radzen;

namespace ITStockM.Services.DeliveryOrders;

public interface IDeliveryOrderService
{
    Task<IQueryable<DeliveryOrder>> GetDeliveryOrders(Query query = null);
    Task<List<DeliveryOrder>> GetDeliveryOrdersList(Query query = null);
    Task<DeliveryOrder?> GetDeliveryOrderByNumber(string deleveryOrderNumber);
    Task<DeliveryOrder> CreateDeliveryOrder(DeliveryOrder deliveryorder);
    Task<DeliveryOrder> UpdateDeliveryOrder(string deleveryordernumber, DeliveryOrder deliveryorder);
    Task<DeliveryOrder> DeleteDeliveryOrder(string deleveryordernumber);
}
