using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using ITStockM.Models.ITStockManagment;
using ITStockM.Repositories;
using ITStockM.Services.Interfaces;
using ITStockM.Services.Utilities;
using Radzen;

namespace ITStockM.Services.Assignments;

public class AssignmentService : IAssignmentService
{
    private readonly IAssignmentRepository assignmentRepository;
    private readonly IOperationNotificationService? operationNotificationService;

    public AssignmentService(IAssignmentRepository assignmentRepository, IOperationNotificationService? operationNotificationService = null)
    {
        this.assignmentRepository = assignmentRepository;
        this.operationNotificationService = operationNotificationService;
    }

    public async Task<IQueryable<Assignment>> GetAssignments(Query query = null)
    {
        IQueryable<Assignment> items = assignmentRepository.QueryWithIncludes();

        if (query != null)
        {
            if (!string.IsNullOrEmpty(query.Expand))
            {
                var propertiesToExpand = query.Expand.Split(',');
                foreach (var p in propertiesToExpand)
                {
                    items = Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.Include(items, p.Trim());
                }
            }

            items = items.ApplyQuery(query);
        }

        return await Task.FromResult(items);
    }

    public async Task<Assignment?> GetAssignmentById(int id)
    {
        var item = await assignmentRepository.GetByIdWithRelatedAsync(id);
        return item;
    }

    public async Task<Assignment> CreateAssignment(Assignment assignment)
    {
        var existingItem = assignmentRepository.Query().FirstOrDefault(i => i.Id == assignment.Id);
        if (existingItem != null)
        {
            throw new Exception("Item already available");
        }

        await assignmentRepository.AddAsync(assignment);
        await assignmentRepository.SaveChangesAsync();

        _ = Task.Run(async () =>
        {
            if (operationNotificationService != null)
                await operationNotificationService.NotifyAssignmentCreated(assignment);
        });

        return assignment;
    }

    public async Task<Assignment> UpdateAssignment(int id, Assignment assignment)
    {
            var itemToUpdate = assignmentRepository.Query().FirstOrDefault(i => i.Id == assignment.Id);

        var entryToUpdate = assignmentRepository is EfRepository<Assignment> ef ? ef.Query().FirstOrDefault(i => i.Id == assignment.Id) : itemToUpdate;

        // Use repository's context to update values
        var ctx = (assignmentRepository as dynamic)?._context as Microsoft.EntityFrameworkCore.DbContext;
        var entry = ctx.Entry(itemToUpdate);
        entry.CurrentValues.SetValues(assignment);
        entry.State = EntityState.Modified;

        await assignmentRepository.SaveChangesAsync();

        _ = Task.Run(async () =>
        {
            if (operationNotificationService != null)
                await operationNotificationService.NotifyAssignmentUpdated(assignment);
        });

        return assignment;
    }

    public async Task<Assignment> DeleteAssignment(int id)
    {
            var itemToDelete = assignmentRepository.QueryWithIncludes().FirstOrDefault(i => i.Id == id);

        assignmentRepository.Remove(itemToDelete);
        await assignmentRepository.SaveChangesAsync();

        _ = Task.Run(async () =>
        {
            if (operationNotificationService != null)
                await operationNotificationService.NotifyAssignmentDeleted(id);
        });

        return itemToDelete;
    }
}
