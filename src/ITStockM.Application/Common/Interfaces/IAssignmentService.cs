using System.Linq;
using ITStockM.Application.Common.Models;
using ITStockM.Domain.Entities;

namespace ITStockM.Services.Assignments;

public interface IAssignmentService
{
    Task<IQueryable<Assignment>> GetAssignments(QueryOptions? query = null);
    Task<Assignment?> GetAssignmentById(int id);
    Task<Assignment> CreateAssignment(Assignment assignment);
    Task<Assignment> UpdateAssignment(int id, Assignment assignment);
    Task<Assignment> DeleteAssignment(int id);
}
