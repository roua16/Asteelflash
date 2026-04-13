using ITStockM.Domain.Entities;

namespace ITStockM.Repositories;

public interface IMaterielRepository : IRepository<Materiel>
{
    Task<Materiel?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
}
