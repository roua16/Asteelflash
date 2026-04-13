using System.Linq;
using System.Linq.Dynamic.Core;
using ITStockM.Data;
using ITStockM.Domain.Entities;
using ITStockM.Domain.Exceptions;
using ITStockM.Repositories;
using ITStockM.Services.Utilities;
using Microsoft.EntityFrameworkCore;
using Radzen;

namespace ITStockM.Services.Suppliers;

public class SupplierService : ISupplierService
{
    private readonly IRepository<Supplier> supplierRepository;

    public SupplierService(IRepository<Supplier> supplierRepository)
    {
        this.supplierRepository = supplierRepository;
    }

    public Task<IQueryable<Supplier>> GetSuppliers(Query query = null)
    {
        IQueryable<Supplier> items = supplierRepository.Query();

        if (query != null)
        {
            if (!string.IsNullOrEmpty(query.Expand))
            {
                var propertiesToExpand = query.Expand.Split(',');
                foreach (var p in propertiesToExpand)
                {
                    items = items.Include(p.Trim());
                }
            }

            items = items.ApplyQuery(query);
        }

        return Task.FromResult(items);
    }

    public async Task<List<Supplier>> GetSuppliersList(Query query = null)
    {
        var items = await GetSuppliers(query);
        return await items.ToListAsync();
    }

    public async Task<Supplier?> GetSupplierBySupplierName(string supplierName)
    {
        return await supplierRepository.Query().AsNoTracking().FirstOrDefaultAsync(i => i.SupplierName == supplierName);
    }

    public async Task<Supplier> CreateSupplier(Supplier supplier)
    {
        var existingSupplier = await supplierRepository.Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.SupplierName == supplier.SupplierName);

        if (existingSupplier != null)
        {
            throw new InvalidOperationException($"Supplier '{supplier.SupplierName}' already exists.");
        }

        await supplierRepository.AddAsync(supplier);
        await supplierRepository.SaveChangesAsync();
        return supplier;
    }

    public async Task<Supplier> UpdateSupplier(string supplierName, Supplier supplier)
    {
        var itemToUpdate = await supplierRepository.Query().FirstOrDefaultAsync(i => i.SupplierName == supplierName);
        if (itemToUpdate == null)
        {
            throw new BusinessRuleViolationException("Item no longer available");
        }

        if (!string.Equals(supplierName, supplier.SupplierName, StringComparison.OrdinalIgnoreCase))
        {
            var duplicateName = await supplierRepository.Query()
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.SupplierName == supplier.SupplierName);

            if (duplicateName != null)
            {
                throw new InvalidOperationException($"Supplier '{supplier.SupplierName}' already exists.");
            }
        }

        itemToUpdate.SupplierName = supplier.SupplierName;
        itemToUpdate.Adress = supplier.Adress;
        itemToUpdate.Email = supplier.Email;
        itemToUpdate.PhoneNumber = supplier.PhoneNumber;

        supplierRepository.Update(itemToUpdate);
        await supplierRepository.SaveChangesAsync();
        return itemToUpdate;
    }

    public async Task<Supplier> DeleteSupplier(string supplierName)
    {
        var itemToDelete = supplierRepository.Query()
            .Include(i => i.DeliveryOrders)
            .Include(i => i.Offers)
            .FirstOrDefault(i => i.SupplierName == supplierName);

        if (itemToDelete == null)
        {
            throw new BusinessRuleViolationException("Item no longer available");
        }

        supplierRepository.Remove(itemToDelete);
        await supplierRepository.SaveChangesAsync();
        return itemToDelete;
    }
}
