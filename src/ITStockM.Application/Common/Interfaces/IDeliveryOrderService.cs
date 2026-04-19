using System.Linq;
using ITStockM.Application.Common.Models;
using ITStockM.Domain.Entities;

namespace ITStockM.Services.DeliveryOrders;

public interface IDeliveryOrderService
{
    Task<IQueryable<DeliveryOrder>> GetDeliveryOrders(QueryOptions? query = null);
    Task<List<DeliveryOrder>> GetDeliveryOrdersList(QueryOptions? query = null);
    Task<DeliveryOrder?> GetDeliveryOrderByNumber(string deleveryOrderNumber);
    Task<DeliveryOrder> CreateDeliveryOrder(DeliveryOrder deliveryorder);
    Task<DeliveryOrder> UpdateDeliveryOrder(string deleveryordernumber, DeliveryOrder deliveryorder);
    Task<DeliveryOrder> DeleteDeliveryOrder(string deleveryordernumber);
}
