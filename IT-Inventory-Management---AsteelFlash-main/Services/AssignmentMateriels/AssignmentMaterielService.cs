using ITStockM.Models.ITStockManagment;
using Radzen;

namespace ITStockM.Services.AssignmentMateriels;

public class AssignmentMaterielService : IAssignmentMaterielService
{
    private readonly ITStockManagmentService stockService;

    public AssignmentMaterielService(ITStockManagmentService stockService)
    {
        this.stockService = stockService;
    }

    public Task<IQueryable<AssignmentMateriel>> GetAssignmentMateriels(Query query = null)
        => stockService.GetAssignmentMateriels(query);

    public Task<AssignmentMateriel?> GetAssignmentMaterielByMaterielIdAndAssignmentId(int materielId, int assignmentId)
        => stockService.GetAssignmentMaterielByMaterielIdAndAssignmentId(materielId, assignmentId);

    public Task<AssignmentMateriel> CreateAssignmentMateriel(AssignmentMateriel assignmentMateriel)
        => stockService.CreateAssignmentMateriel(assignmentMateriel);

    public Task<AssignmentMateriel> UpdateAssignmentMateriel(int materielId, int assignmentId, AssignmentMateriel assignmentMateriel)
        => stockService.UpdateAssignmentMateriel(materielId, assignmentId, assignmentMateriel);

    public Task<AssignmentMateriel> DeleteAssignmentMateriel(int materielId, int assignmentId)
        => stockService.DeleteAssignmentMateriel(materielId, assignmentId);
}
