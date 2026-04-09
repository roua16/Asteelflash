using ITStockM.Models.ITStockManagment;
using Radzen;

namespace ITStockM.Services.Suppliers;

public interface ISupplierService
{
    Task<IQueryable<Supplier>> GetSuppliers(Query query = null);
    Task<List<Supplier>> GetSuppliersList(Query query = null);
    Task<Supplier?> GetSupplierBySupplierName(string supplierName);
    Task<Supplier> CreateSupplier(Supplier supplier);
    Task<Supplier> UpdateSupplier(string supplierName, Supplier supplier);
    Task<Supplier> DeleteSupplier(string supplierName);
}
