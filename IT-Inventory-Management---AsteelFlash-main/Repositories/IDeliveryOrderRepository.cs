using ITStockM.Models.ITStockManagment;

namespace ITStockM.Repositories;

public interface IDeliveryOrderRepository : IRepository<DeliveryOrder>
{
    IQueryable<DeliveryOrder> QueryWithIncludes();
    Task<DeliveryOrder?> GetByNumberWithRelatedAsync(string deleveryOrderNumber, CancellationToken cancellationToken = default);
}
