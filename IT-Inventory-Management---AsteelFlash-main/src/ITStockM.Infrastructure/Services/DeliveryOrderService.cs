using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using ITStockM.Domain.Entities;
using ITStockM.Domain.Exceptions;
using ITStockM.Repositories;
using ITStockM.Services.Interfaces;
using ITStockM.Services.Utilities;
using Microsoft.Extensions.DependencyInjection;
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

        if (query != null)
        {
            items = items.ApplyQuery(query);
        }

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
            items = items.ApplyQuery(query);
        }

        return await items.ToListAsync();
    }

    public async Task<DeliveryOrder?> GetDeliveryOrderByNumber(string deleveryOrderNumber)
    {
        return await deliveryOrderRepository.GetByNumberWithRelatedAsync(deleveryOrderNumber);
    }

    public async Task<DeliveryOrder> CreateDeliveryOrder(DeliveryOrder deliveryorder)
    {
        var existingItem = await deliveryOrderRepository.Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.DeleveryOrderNumber == deliveryorder.DeleveryOrderNumber);

        if (existingItem != null)
        {
            throw new System.InvalidOperationException($"Delivery order '{deliveryorder.DeleveryOrderNumber}' already exists.");
        }

        await deliveryOrderRepository.AddAsync(deliveryorder);
        await deliveryOrderRepository.SaveChangesAsync();

        if (operationNotificationService != null)
            await operationNotificationService.NotifyDeliveryOrderCreated(deliveryorder);

        return deliveryorder;
    }

    public async Task<DeliveryOrder> UpdateDeliveryOrder(string deleveryordernumber, DeliveryOrder deliveryorder)
    {
        var itemToUpdate = await deliveryOrderRepository.Query().FirstOrDefaultAsync(i => i.DeleveryOrderNumber == deleveryordernumber);
        if (itemToUpdate == null)
        {
            throw new BusinessRuleViolationException("Item no longer available");
        }

        if (!string.Equals(deleveryordernumber, deliveryorder.DeleveryOrderNumber, StringComparison.OrdinalIgnoreCase))
        {
            var duplicateNumber = await deliveryOrderRepository.Query()
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.DeleveryOrderNumber == deliveryorder.DeleveryOrderNumber);

            if (duplicateNumber != null)
            {
                throw new System.InvalidOperationException($"Delivery order '{deliveryorder.DeleveryOrderNumber}' already exists.");
            }
        }

        itemToUpdate.DeleveryOrderNumber = deliveryorder.DeleveryOrderNumber;
        itemToUpdate.OrderNumber = deliveryorder.OrderNumber;
        itemToUpdate.Descriptoin = deliveryorder.Descriptoin;
        itemToUpdate.SupplierName = deliveryorder.SupplierName;
        itemToUpdate.Date = deliveryorder.Date;
        itemToUpdate.DeliveryDate = deliveryorder.DeliveryDate;
        itemToUpdate.EmployeeId = deliveryorder.EmployeeId;
        itemToUpdate.HasDelayedM = deliveryorder.HasDelayedM;

        deliveryOrderRepository.Update(itemToUpdate);

        await deliveryOrderRepository.SaveChangesAsync();

        if (operationNotificationService != null)
            await operationNotificationService.NotifyDeliveryOrderUpdated(itemToUpdate);

        return itemToUpdate;
    }

    public async Task<DeliveryOrder> DeleteDeliveryOrder(string deleveryordernumber)
    {
            var itemToDelete = deliveryOrderRepository.QueryWithIncludes().FirstOrDefault(i => i.DeleveryOrderNumber == deleveryordernumber);
        if (itemToDelete == null)
        {
            throw new BusinessRuleViolationException("Item no longer available");
        }

        deliveryOrderRepository.Remove(itemToDelete);
        await deliveryOrderRepository.SaveChangesAsync();

        if (operationNotificationService != null)
            await operationNotificationService.NotifyDeliveryOrderDeleted(deleveryordernumber);

        return itemToDelete;
    }
}
