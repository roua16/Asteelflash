using Microsoft.EntityFrameworkCore;
using ITStockM.Data;
using ITStockM.Models.ITStockManagment;

namespace ITStockM.Repositories;

public class AssignmentRepository : EfRepository<Assignment>, IAssignmentRepository
{
    public AssignmentRepository(ITStockManagmentContext context) : base(context)
    {
    }

    public IQueryable<Assignment> QueryWithIncludes()
    {
        return _dbSet.Include(i => i.Employee)
                     .Include(i => i.AssignedEmployee)
                     .Include(i => i.Project)
                     .Include(i => i.AssignmentMateriels).ThenInclude(am => am.Materiel)
                     .AsQueryable();
    }

    public async Task<Assignment?> GetByIdWithRelatedAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(i => i.Employee)
            .Include(i => i.AssignedEmployee)
            .Include(i => i.Project)
            .Include(i => i.AssignmentMateriels).ThenInclude(am => am.Materiel)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }
}
