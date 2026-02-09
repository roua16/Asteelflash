using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using ITStockM.Models.ITStockManagment;
using ITStockM.Repositories;
using ITStockM.Services.Interfaces;
using ITStockM.Services.Utilities;
using Radzen;

namespace ITStockM.Services.Materiels;

public class MaterielService : IMaterielService
{
    private readonly IMaterielRepository materielRepository;
    private readonly IOperationNotificationService? operationNotificationService;

    public MaterielService(IMaterielRepository materielRepository, IOperationNotificationService? operationNotificationService = null)
    {
        this.materielRepository = materielRepository;
        this.operationNotificationService = operationNotificationService;
    }

    public async Task<IQueryable<Materiel>> GetMateriels(Query query = null)
    {
        IQueryable<Materiel> items = materielRepository.Query();
        items = items.Include(i => i.AssignmentMateriels).ThenInclude(i => i.Assignment);

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

    public async Task<Materiel?> GetMaterielById(int id)
    {
        return await materielRepository.GetByIdAsync(id);
    }

    public async Task<Materiel?> GetMaterielByName(string name)
    {
        return await materielRepository.GetByNameAsync(name);
    }

    public async Task<Materiel> CreateMateriel(Materiel materiel)
    {
            var existingItem = materielRepository.Query().FirstOrDefault(i => i.Id == materiel.Id);

        await materielRepository.AddAsync(materiel);
        await materielRepository.SaveChangesAsync();

        _ = Task.Run(async () =>
        {
            if (operationNotificationService != null)
                await operationNotificationService.NotifyMaterielCreated(materiel);
        });

        return materiel;
    }

    public async Task<Materiel> UpdateMateriel(int id, Materiel materiel)
    {
            var itemToUpdate = materielRepository.Query().FirstOrDefault(i => i.Id == materiel.Id);

        var ctx = (materielRepository as dynamic)?._context as Microsoft.EntityFrameworkCore.DbContext;
        var entry = ctx.Entry(itemToUpdate);
        entry.CurrentValues.SetValues(materiel);
        entry.State = EntityState.Modified;

        await materielRepository.SaveChangesAsync();

        _ = Task.Run(async () =>
        {
            if (operationNotificationService != null)
                await operationNotificationService.NotifyMaterielUpdated(materiel);
        });

        return materiel;
    }

    public async Task<Materiel> DeleteMateriel(int id)
    {
            var query = materielRepository.Query().Include(i => i.AssignmentMateriels).Include(i => i.DeliveryOrderMateriels);
            var itemToDelete = query.FirstOrDefault(i => i.Id == id);

        materielRepository.Remove(itemToDelete);
        await materielRepository.SaveChangesAsync();

        _ = Task.Run(async () =>
        {
            if (operationNotificationService != null)
                await operationNotificationService.NotifyMaterielDeleted(itemToDelete);
        });

        return itemToDelete;
    }
}
