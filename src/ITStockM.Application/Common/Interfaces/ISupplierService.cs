using ITStockM.Application.Common.Models;
using ITStockM.Domain.Entities;

namespace ITStockM.Services.Suppliers;

public interface ISupplierService
{
    Task<IQueryable<Supplier>> GetSuppliers(QueryOptions? query = null);
    Task<List<Supplier>> GetSuppliersList(QueryOptions? query = null);
    Task<Supplier?> GetSupplierBySupplierName(string supplierName);
    Task<Supplier> CreateSupplier(Supplier supplier);
    Task<Supplier> UpdateSupplier(string supplierName, Supplier supplier);
    Task<Supplier> DeleteSupplier(string supplierName);
}
