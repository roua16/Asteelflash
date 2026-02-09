using ITStockM.Models.ITStockManagment;

namespace ITStockM.Repositories;

public interface IMaterielRepository : IRepository<Materiel>
{
    Task<Materiel?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
}
