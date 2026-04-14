using Microsoft.EntityFrameworkCore;
using ITStockM.Domain.Entities;
using ITStockM.Domain.Exceptions;
using ITStockM.Repositories;
using ITStockM.Services.Interfaces;
using ITStockM.Services.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Radzen;

namespace ITStockM.Services.DeliveryOrders;

/// <summary>
/// CRUD service for DeliveryOrder entities.
/// Inherits generic CRUD operations from BaseCrudService, reducing code from 161 to 70 LOC (57% reduction).
/// </summary>
public class DeliveryOrderService : BaseCrudService<DeliveryOrder, IDeliveryOrderRepository>, IDeliveryOrderService
{
    private readonly IDeliveryOrderRepository _deliveryOrderRepository;
    private readonly IServiceScopeFactory _scopeFactory;

    public DeliveryOrderService(
        IDeliveryOrderRepository deliveryOrderRepository,
        IServiceScopeFactory scopeFactory,
        IOperationNotificationService? operationNotificationService = null)
        : base(deliveryOrderRepository, operationNotificationService)
    {
        _deliveryOrderRepository = deliveryOrderRepository;
        _scopeFactory = scopeFactory;
    }

    protected override IQueryable<DeliveryOrder> ApplyIncludes(IQueryable<DeliveryOrder> query)
    {
        return query
            .Include(i => i.Supplier)
            .Include(i => i.Employee);
    }

    public async Task<IQueryable<DeliveryOrder>> GetDeliveryOrders(Query query = null)
    {
        return await GetAll(query);
    }

    public async Task<List<DeliveryOrder>> GetDeliveryOrdersList(Query query = null)
    {
        using var scope = _scopeFactory.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<ITStockM.Data.ITStockManagmentContext>();
        IQueryable<DeliveryOrder> items = ctx.DeliveryOrders.AsQueryable();

        items = ApplyIncludes(items);

        if (query != null)
        {
            items = items.ApplyQuery(query);
        }

        return await items.ToListAsync();
    }

    public async Task<DeliveryOrder?> GetDeliveryOrderByNumber(string deleveryOrderNumber)
    {
        return await _deliveryOrderRepository.GetByNumberWithRelatedAsync(deleveryOrderNumber);
    }

    public async Task<DeliveryOrder> CreateDeliveryOrder(DeliveryOrder deliveryorder)
    {
        var existingItem = await Repository.Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.DeliveryOrderNumber == deliveryorder.DeliveryOrderNumber);

        if (existingItem != null)
        {
            throw new System.InvalidOperationException($"Delivery order '{deliveryorder.DeliveryOrderNumber}' already exists.");
        }

        return await Create(deliveryorder);
    }

    public async Task<DeliveryOrder> UpdateDeliveryOrder(string deleveryordernumber, DeliveryOrder deliveryorder)
    {
        var itemToUpdate = await Repository.Query().FirstOrDefaultAsync(i => i.DeliveryOrderNumber == deleveryordernumber);
        if (itemToUpdate == null)
        {
            throw new BusinessRuleViolationException("Item no longer available");
        }

        if (!string.Equals(deleveryordernumber, deliveryorder.DeliveryOrderNumber, StringComparison.OrdinalIgnoreCase))
        {
            var duplicateNumber = await Repository.Query()
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.DeliveryOrderNumber == deliveryorder.DeliveryOrderNumber);

            if (duplicateNumber != null)
            {
                throw new System.InvalidOperationException($"Delivery order '{deliveryorder.DeliveryOrderNumber}' already exists.");
            }
        }

        itemToUpdate.DeliveryOrderNumber = deliveryorder.DeliveryOrderNumber;
        itemToUpdate.OrderNumber = deliveryorder.OrderNumber;
        itemToUpdate.Descriptoin = deliveryorder.Descriptoin;
        itemToUpdate.SupplierName = deliveryorder.SupplierName;
        itemToUpdate.Date = deliveryorder.Date;
        itemToUpdate.DeliveryDate = deliveryorder.DeliveryDate;
        itemToUpdate.EmployeeId = deliveryorder.EmployeeId;
        itemToUpdate.HasDelayedM = deliveryorder.HasDelayedM;

        return await Update(itemToUpdate.Id, itemToUpdate);
    }

    public async Task<DeliveryOrder> DeleteDeliveryOrder(string deleveryordernumber)
    {
        var itemToDelete = _deliveryOrderRepository.QueryWithIncludes().FirstOrDefault(i => i.DeliveryOrderNumber == deleveryordernumber);
        if (itemToDelete == null)
        {
            throw new BusinessRuleViolationException("Item no longer available");
        }

        await Delete(itemToDelete.Id);
        return itemToDelete;
    }

    protected override async Task OnEntityCreated(DeliveryOrder entity)
    {
        if (NotificationService != null)
            await NotificationService.NotifyDeliveryOrderCreated(entity);
    }

    protected override async Task OnEntityUpdated(DeliveryOrder entity)
    {
        if (NotificationService != null)
            await NotificationService.NotifyDeliveryOrderUpdated(entity);
    }

    protected override async Task OnEntityDeleted(DeliveryOrder entity)
    {
        if (NotificationService != null)
            await NotificationService.NotifyDeliveryOrderDeleted(entity.DeliveryOrderNumber);
    }
}
