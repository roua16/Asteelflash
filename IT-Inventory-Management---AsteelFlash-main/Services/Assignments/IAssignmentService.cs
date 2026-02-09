using System.Linq;
using ITStockM.Models.ITStockManagment;
using Radzen;

namespace ITStockM.Services.Assignments;

public interface IAssignmentService
{
    Task<IQueryable<Assignment>> GetAssignments(Query query = null);
    Task<Assignment?> GetAssignmentById(int id);
    Task<Assignment> CreateAssignment(Assignment assignment);
    Task<Assignment> UpdateAssignment(int id, Assignment assignment);
    Task<Assignment> DeleteAssignment(int id);
}
