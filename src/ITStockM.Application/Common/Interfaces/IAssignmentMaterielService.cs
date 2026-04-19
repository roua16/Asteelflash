using ITStockM.Application.Common.Models;
using ITStockM.Domain.Entities;

namespace ITStockM.Services.AssignmentMateriels;

public interface IAssignmentMaterielService
{
    Task<IQueryable<AssignmentMateriel>> GetAssignmentMateriels(QueryOptions? query = null);
    Task<AssignmentMateriel?> GetAssignmentMaterielByMaterielIdAndAssignmentId(int materielId, int assignmentId);
    Task<AssignmentMateriel> CreateAssignmentMateriel(AssignmentMateriel assignmentMateriel);
    Task<AssignmentMateriel> UpdateAssignmentMateriel(int materielId, int assignmentId, AssignmentMateriel assignmentMateriel);
    Task<AssignmentMateriel> DeleteAssignmentMateriel(int materielId, int assignmentId);
}
