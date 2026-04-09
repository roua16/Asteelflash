using ITStockM.Models.ITStockManagment;
using Radzen;

namespace ITStockM.Services.AssignmentMateriels;

public interface IAssignmentMaterielService
{
    Task<IQueryable<AssignmentMateriel>> GetAssignmentMateriels(Query query = null);
    Task<AssignmentMateriel?> GetAssignmentMaterielByMaterielIdAndAssignmentId(int materielId, int assignmentId);
    Task<AssignmentMateriel> CreateAssignmentMateriel(AssignmentMateriel assignmentMateriel);
    Task<AssignmentMateriel> UpdateAssignmentMateriel(int materielId, int assignmentId, AssignmentMateriel assignmentMateriel);
    Task<AssignmentMateriel> DeleteAssignmentMateriel(int materielId, int assignmentId);
}
