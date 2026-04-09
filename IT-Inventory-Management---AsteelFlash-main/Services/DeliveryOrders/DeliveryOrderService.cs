using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using ITStockM.Models.ITStockManagment;
using ITStockM.Repositories;
using ITStockM.Services.Interfaces;
using ITStockM.Services.Utilities;
using Radzen;

namespace ITStockM.Services.DeliveryOrders;

public class DeliveryOrderService : IDeliveryOrderService
{
    private readonly IDeliveryOrderRepository deliveryOrderRepository;
    private readonly IServiceScopeFactory scopeFactory;
    private readonly IOperationNotificationService? operationNotificationService;

    public DeliveryOrderService(IDeliveryOrderRepository deliveryOrderRepository, IServiceScopeFactory scopeFactory, IOperationNotificationService? operationNotificationService = null)
    {
        this.deliveryOrderRepository = deliveryOrderRepository;
        this.scopeFactory = scopeFactory;
        this.operationNotificationService = operationNotificationService;
    }

    public async Task<IQueryable<DeliveryOrder>> GetDeliveryOrders(Query query = null)
    {
        IQueryable<DeliveryOrder> items = deliveryOrderRepository.Query();

        if (query != null && !string.IsNullOrEmpty(query.Expand))
        {
            var propertiesToExpand = query.Expand.Split(',');
            foreach (var p in propertiesToExpand)
            {
                items = Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.Include(items, p.Trim());
            }
        }
        else
        {
            items = items.Include(i => i.Supplier);
            items = items.Include(i => i.Employee);
        }

        items = items.ApplyQuery(query);

        return await Task.FromResult(items);
    }

    public async Task<List<DeliveryOrder>> GetDeliveryOrdersList(Query query = null)
    {
        using var scope = scopeFactory.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<ITStockM.Data.ITStockManagmentContext>();
        IQueryable<DeliveryOrder> items = ctx.DeliveryOrders.AsQueryable();

        if (query != null && !string.IsNullOrEmpty(query.Expand))
        {
            var propertiesToExpand = query.Expand.Split(',');
            foreach (var p in propertiesToExpand)
            {
                items = Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.Include(items, p.Trim());
            }
        }
        else
        {
            items = items.Include(i => i.Supplier);
            items = items.Include(i => i.Employee);
        }

            if (query != null)
            {
                if (!string.IsNullOrEmpty(query.Filter))
                {
                    // Dynamic filtering temporarily disabled for reliability across frameworks.
                    // TODO: re-enable dynamic filtering when System.Linq.Dynamic.Core compatibility is confirmed.
                }

                if (!string.IsNullOrEmpty(query.OrderBy))
                    items = items.OrderBy(query.OrderBy);

                if (query.Skip.HasValue)
                    items = items.Skip(query.Skip.Value);

                if (query.Top.HasValue)
                    items = items.Take(query.Top.Value);
            }

        return await items.ToListAsync();
    }

    public async Task<DeliveryOrder?> GetDeliveryOrderByNumber(string deleveryOrderNumber)
    {
        return await deliveryOrderRepository.GetByNumberWithRelatedAsync(deleveryOrderNumber);
    }

    public async Task<DeliveryOrder> CreateDeliveryOrder(DeliveryOrder deliveryorder)
    {
            var existingItem = deliveryOrderRepository.Query().FirstOrDefault(i => i.DeleveryOrderNumber == deliveryorder.DeleveryOrderNumber);

        await deliveryOrderRepository.AddAsync(deliveryorder);
        await deliveryOrderRepository.SaveChangesAsync();

        if (operationNotificationService != null)
            await operationNotificationService.NotifyDeliveryOrderCreated(deliveryorder);

        return deliveryorder;
    }

    public async Task<DeliveryOrder> UpdateDeliveryOrder(string deleveryordernumber, DeliveryOrder deliveryorder)
    {
            var itemToUpdate = deliveryOrderRepository.Query().FirstOrDefault(i => i.DeleveryOrderNumber == deliveryorder.DeleveryOrderNumber);
            if (itemToUpdate == null)
            {
                throw new Exception("Item no longer available");
            }

        deliveryOrderRepository.Update(deliveryorder);

        await deliveryOrderRepository.SaveChangesAsync();

        if (operationNotificationService != null)
            await operationNotificationService.NotifyDeliveryOrderUpdated(deliveryorder);

        return deliveryorder;
    }

    public async Task<DeliveryOrder> DeleteDeliveryOrder(string deleveryordernumber)
    {
            var itemToDelete = deliveryOrderRepository.QueryWithIncludes().FirstOrDefault(i => i.DeleveryOrderNumber == deleveryordernumber);
        if (itemToDelete == null)
        {
            throw new Exception("Item no longer available");
        }

        deliveryOrderRepository.Remove(itemToDelete);
        await deliveryOrderRepository.SaveChangesAsync();

        if (operationNotificationService != null)
            await operationNotificationService.NotifyDeliveryOrderDeleted(deleveryordernumber);

        return itemToDelete;
    }
}
