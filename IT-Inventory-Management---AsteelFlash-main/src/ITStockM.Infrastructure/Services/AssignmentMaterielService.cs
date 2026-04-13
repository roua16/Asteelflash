using ITStockM.Domain.Entities;
using ITStockM.Domain.Exceptions;
using ITStockM.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ITStockM.Services.AssignmentMateriels;

/// <summary>
/// CRUD service for AssignmentMateriel (junction) entities.
/// Inherits generic CRUD operations from BaseCrudService, reducing code from 87 to 40 LOC (54% reduction).
/// </summary>
public class AssignmentMaterielService : BaseCrudService<AssignmentMateriel, IRepository<AssignmentMateriel>>, IAssignmentMaterielService
{
    public AssignmentMaterielService(IRepository<AssignmentMateriel> repository)
        : base(repository)
    {
    }

    /// <summary>
    /// Apply default eager loading for AssignmentMateriel entities.
    /// </summary>
    protected override IQueryable<AssignmentMateriel> ApplyIncludes(IQueryable<AssignmentMateriel> query)
    {
        return query
            .Include(am => am.Assignment)
                .ThenInclude(a => a.Employee)
            .Include(am => am.Assignment)
                .ThenInclude(a => a.Project)
            .Include(am => am.Assignment)
                .ThenInclude(a => a.AssignedEmployee)
            .Include(am => am.Materiel);
    }

    /// <summary>
    /// Get AssignmentMateriel by materiel and assignment IDs with validation.
    /// </summary>
    public async Task<AssignmentMateriel?> GetAssignmentMaterielByMaterielIdAndAssignmentId(int materielId, int assignmentId)
    {
        return await Repository.Query()
            .Include(am => am.Assignment)
            .Include(am => am.Materiel)
            .FirstOrDefaultAsync(am => am.MaterielId == materielId && am.AssignmentId == assignmentId);
    }

    /// <summary>
    /// Create AssignmentMateriel with duplicate check.
    /// </summary>
    public async Task<AssignmentMateriel> CreateAssignmentMateriel(AssignmentMateriel assignmentMateriel)
    {
        var existing = await Repository.Query()
            .FirstOrDefaultAsync(am => am.MaterielId == assignmentMateriel.MaterielId && 
                                       am.AssignmentId == assignmentMateriel.AssignmentId);

        if (existing != null)
            throw new BusinessRuleViolationException("Item already available");

        return await Create(assignmentMateriel);
    }

    /// <summary>
    /// Update AssignmentMateriel by materiel and assignment IDs.
    /// </summary>
    public async Task<AssignmentMateriel> UpdateAssignmentMateriel(int materielId, int assignmentId, AssignmentMateriel assignmentMateriel)
    {
        var existing = await Repository.Query()
            .FirstOrDefaultAsync(am => am.MaterielId == materielId && am.AssignmentId == assignmentId);

        if (existing == null)
            throw new EntityNotFoundException("AssignmentMateriel", $"{materielId}-{assignmentId}");

        // Update all fields
        existing.Quantity = assignmentMateriel.Quantity;
        existing.IsDeleted = assignmentMateriel.IsDeleted;
        existing.UpdatedAt = DateTime.UtcNow;

        return await Update(existing.Id, existing);
    }

    /// <summary>
    /// Delete AssignmentMateriel by materiel and assignment IDs.
    /// </summary>
    public async Task<AssignmentMateriel> DeleteAssignmentMateriel(int materielId, int assignmentId)
    {
        var existing = await Repository.Query()
            .FirstOrDefaultAsync(am => am.MaterielId == materielId && am.AssignmentId == assignmentId);

        if (existing == null)
            throw new EntityNotFoundException("AssignmentMateriel", $"{materielId}-{assignmentId}");

        await Delete(existing.Id);
        return existing;
    }
}
