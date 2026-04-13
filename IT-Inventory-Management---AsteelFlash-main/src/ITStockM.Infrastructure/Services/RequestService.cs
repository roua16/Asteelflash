using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using ITStockM.Domain.Entities;
using ITStockM.Repositories;
using ITStockM.Services.Interfaces;
using ITStockM.Services.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Radzen;

namespace ITStockM.Services.Requests;

public class RequestService : IRequestService
{
    private readonly IRequestRepository requestRepository;
    private readonly IServiceScopeFactory scopeFactory;
    private readonly IOperationNotificationService? operationNotificationService;

    public RequestService(IRequestRepository requestRepository, IServiceScopeFactory scopeFactory, IOperationNotificationService? operationNotificationService = null)
    {
        this.requestRepository = requestRepository;
        this.scopeFactory = scopeFactory;
        this.operationNotificationService = operationNotificationService;
    }

    public async Task<IQueryable<Request>> GetRequests(Query query = null)
    {
        IQueryable<Request> items = requestRepository.QueryWithIncludes();

        if (query != null && !string.IsNullOrEmpty(query.Expand))
        {
            var propertiesToExpand = query.Expand.Split(',');
            foreach (var p in propertiesToExpand)
            {
                items = Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.Include(items, p.Trim());
            }
        }

        items = items.ApplyQuery(query);

        return await Task.FromResult(items);
    }

    public async Task<List<Request>> GetRequestsList(Query query = null)
    {
        using var scope = scopeFactory.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<ITStockM.Data.ITStockManagmentContext>();
            var items = ctx.Requests.Include(r => r.Employee).AsQueryable();

            if (query != null)
            {
                if (!string.IsNullOrEmpty(query.Filter))
                {
                    // Dynamic filtering temporarily disabled for reliability across frameworks.
                    // TODO: re-enable dynamic filtering when System.Linq.Dynamic.Core compatibility is confirmed.
                }
            if (query.Skip.HasValue)
                items = items.Skip(query.Skip.Value);

            if (query.Top.HasValue)
                items = items.Take(query.Top.Value);
        }

        return await items.ToListAsync();
    }

    public async Task<Request?> GetRequestById(int id)
    {
        return await requestRepository.GetByIdWithRelatedAsync(id);
    }

    public async Task<Request> CreateRequest(Request request)
    {
        // Ensure consistent behavior from previous service
        request.File ??= Array.Empty<byte>();

        await requestRepository.AddAsync(request);
        await requestRepository.SaveChangesAsync();

        if (operationNotificationService != null)
            await operationNotificationService.NotifyRequestCreated(request);

        return request;
    }

    public async Task<Request> UpdateRequest(int id, Request request)
    {
            var itemToUpdate = requestRepository.Query().FirstOrDefault(i => i.Id == request.Id);
            if (itemToUpdate == null)
            {
                throw new Exception("Item no longer available");
            }

        if (request.File == null)
        {
            request.File = itemToUpdate.File;
        }

        requestRepository.Update(request);

        await requestRepository.SaveChangesAsync();

        if (operationNotificationService != null)
            await operationNotificationService.NotifyRequestUpdated(request);

        return request;
    }

    public async Task<Request> DeleteRequest(int id)
    {
        var itemToDelete = requestRepository.QueryWithIncludes().FirstOrDefault(i => i.Id == id);
        if (itemToDelete == null)
            throw new Exception("Item no longer available");

        requestRepository.Remove(itemToDelete);
        await requestRepository.SaveChangesAsync();

        if (operationNotificationService != null)
            await operationNotificationService.NotifyRequestDeleted(id);

        return itemToDelete;
    }
}
