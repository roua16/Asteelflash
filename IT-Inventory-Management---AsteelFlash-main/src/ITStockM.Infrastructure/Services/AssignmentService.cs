using System.Linq;
using System.Linq.Dynamic.Core;
using ITStockM.Domain.Entities;
using ITStockM.Repositories;
using ITStockM.Services.Interfaces;
using ITStockM.Services.Utilities;
using Radzen;

namespace ITStockM.Services.Assignments;

public class AssignmentService : IAssignmentService
{
    private readonly IAssignmentRepository assignmentRepository;
    private readonly IOperationNotificationService? operationNotificationService;

    // Semaphore to serialize access to repository/DbContext to avoid concurrent EF operations in Blazor Server
    private readonly System.Threading.SemaphoreSlim dbSemaphore = new System.Threading.SemaphoreSlim(1, 1);

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
        await dbSemaphore.WaitAsync();
        try
        {
            var item = await assignmentRepository.GetByIdWithRelatedAsync(id);
            return item;
        }
        finally
        {
            dbSemaphore.Release();
        }
    }

    public async Task<Assignment> CreateAssignment(Assignment assignment)
    {
        await dbSemaphore.WaitAsync();
        try
        {
            var existingItem = assignmentRepository.Query().FirstOrDefault(i => i.Id == assignment.Id);
            if (existingItem != null)
            {
                throw new Exception("Item already available");
            }

            await assignmentRepository.AddAsync(assignment);
            await assignmentRepository.SaveChangesAsync();
        }
        finally
        {
            dbSemaphore.Release();
        }

        if (operationNotificationService != null)
            await operationNotificationService.NotifyAssignmentCreated(assignment);

        return assignment;
    }

    public async Task<Assignment> UpdateAssignment(int id, Assignment assignment)
    {
        await dbSemaphore.WaitAsync();
        try
        {
            var itemToUpdate = assignmentRepository.Query().FirstOrDefault(i => i.Id == assignment.Id);
            if (itemToUpdate == null)
            {
                throw new Exception("Item no longer available");
            }

            assignmentRepository.Update(assignment);

            await assignmentRepository.SaveChangesAsync();
        }
        finally
        {
            dbSemaphore.Release();
        }

        if (operationNotificationService != null)
            await operationNotificationService.NotifyAssignmentUpdated(assignment);

        return assignment;
    }

    public async Task<Assignment> DeleteAssignment(int id)
    {
        await dbSemaphore.WaitAsync();
        try
        {
            var itemToDelete = assignmentRepository.QueryWithIncludes().FirstOrDefault(i => i.Id == id);

            if (itemToDelete == null)
            {
                throw new Exception("Item no longer available");
            }

            assignmentRepository.Remove(itemToDelete);
            await assignmentRepository.SaveChangesAsync();

            if (operationNotificationService != null)
                await operationNotificationService.NotifyAssignmentDeleted(id);

            return itemToDelete;
        }
        finally
        {
            dbSemaphore.Release();
        }
    }
}
