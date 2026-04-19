using Microsoft.EntityFrameworkCore;
using ITStockM.Data;
using ITStockM.Domain.Entities;

namespace ITStockM.Repositories;

public class MaterielRepository : EfRepository<Materiel>, IMaterielRepository
{
    public MaterielRepository(ITStockManagmentContext context) : base(context)
    {
    }

    public async Task<Materiel?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(m => m.MaterielName == name, cancellationToken);
    }
}
