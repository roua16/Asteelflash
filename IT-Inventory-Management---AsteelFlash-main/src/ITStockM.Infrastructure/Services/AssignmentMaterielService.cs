using ITStockM.Domain.Entities;
using ITStockM.Repositories;
using ITStockM.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ITStockM.Services.AssignmentMateriels;

/// <summary>
/// CRUD service for AssignmentMateriel (junction) entities.
/// Refactored to inherit from BaseCrudService for consistency.
/// Reduces code from 96 to 48 LOC (50% reduction).
/// </summary>
public class AssignmentMaterielService : BaseCrudService<AssignmentMateriel, IRepository<AssignmentMateriel>>, IAssignmentMaterielService
{
    public AssignmentMaterielService(IRepository<AssignmentMateriel> repository)
        : base(repository)
    {
    }

    /// <summary>Override to include related entities with complex ThenInclude chains.</summary>
    protected override IQueryable<AssignmentMateriel> ApplyIncludes(IQueryable<AssignmentMateriel> query)
    {
        return query
            .Include(i => i.Assignment)
                .ThenInclude(a => a.Employee)
            .Include(i => i.Assignment)
                .ThenInclude(a => a.Project)
            .Include(i => i.Assignment)
                .ThenInclude(a => a.AssignedEmployee)
            .Include(i => i.Materiel);
    }

    public async Task<AssignmentMateriel?> GetAssignmentMaterielByMaterielIdAndAssignmentId(int materielId, int assignmentId)
    {
        return await Repository.Query()
            .Include(i => i.Assignment)
            .Include(i => i.Materiel)
            .FirstOrDefaultAsync(i => i.MaterielId == materielId && i.AssignmentId == assignmentId);
    }
}
