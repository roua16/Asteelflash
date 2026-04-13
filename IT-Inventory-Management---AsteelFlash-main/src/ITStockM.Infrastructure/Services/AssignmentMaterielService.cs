using ITStockM.Domain.Entities;
using ITStockM.Data;
using ITStockM.Services.Utilities;
using Microsoft.EntityFrameworkCore;
using Radzen;

namespace ITStockM.Services.AssignmentMateriels;

public class AssignmentMaterielService : IAssignmentMaterielService
{
    private readonly ITStockManagmentContext _context;

    public AssignmentMaterielService(ITStockManagmentContext context)
    {
        _context = context;
    }

    public async Task<IQueryable<AssignmentMateriel>> GetAssignmentMateriels(Query query = null)
    {
        IQueryable<AssignmentMateriel> items = _context.AssignmentMateriels
            .Include(i => i.Assignment)
            .Include(i => i.Materiel)
            .Include(i => i.Assignment.Employee)
            .Include(i => i.Assignment.Project)
            .Include(i => i.Assignment.AssignedEmployee);

        if (query != null)
        {
            items = items.ApplyQuery(query);
        }

        return await Task.FromResult(items);
    }

    public async Task<AssignmentMateriel?> GetAssignmentMaterielByMaterielIdAndAssignmentId(int materielId, int assignmentId)
    {
        return await _context.AssignmentMateriels
            .Include(i => i.Assignment)
            .Include(i => i.Materiel)
            .FirstOrDefaultAsync(i => i.MaterielId == materielId && i.AssignmentId == assignmentId);
    }

    public async Task<AssignmentMateriel> CreateAssignmentMateriel(AssignmentMateriel assignmentMateriel)
    {
        var existing = await _context.AssignmentMateriels
            .FirstOrDefaultAsync(i => i.MaterielId == assignmentMateriel.MaterielId && i.AssignmentId == assignmentMateriel.AssignmentId);

        if (existing != null)
        {
            throw new InvalidOperationException("Item already available");
        }

        _context.AssignmentMateriels.Add(assignmentMateriel);
        await _context.SaveChangesAsync();
        return assignmentMateriel;
    }

    public async Task<AssignmentMateriel> UpdateAssignmentMateriel(int materielId, int assignmentId, AssignmentMateriel assignmentMateriel)
    {
        var existing = await _context.AssignmentMateriels
            .FirstOrDefaultAsync(i => i.MaterielId == materielId && i.AssignmentId == assignmentId);

        if (existing is null)
        {
            throw new KeyNotFoundException("Item no longer available");
        }

        _context.Entry(existing).CurrentValues.SetValues(assignmentMateriel);
        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<AssignmentMateriel> DeleteAssignmentMateriel(int materielId, int assignmentId)
    {
        var existing = await _context.AssignmentMateriels
            .FirstOrDefaultAsync(i => i.MaterielId == materielId && i.AssignmentId == assignmentId);

        if (existing is null)
        {
            throw new KeyNotFoundException("Item no longer available");
        }

        _context.AssignmentMateriels.Remove(existing);
        await _context.SaveChangesAsync();
        return existing;
    }
}
