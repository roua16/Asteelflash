using System.Linq;
using System.Linq.Dynamic.Core;
using ITStockM.Data;
using ITStockM.Domain.Entities;
using ITStockM.Repositories;
using ITStockM.Services.Utilities;
using Microsoft.EntityFrameworkCore;
using Radzen;

namespace ITStockM.Services.Offers;

public class OfferService : IOfferService
{
    private readonly IRepository<Offer> offerRepository;

    public OfferService(IRepository<Offer> offerRepository)
    {
        this.offerRepository = offerRepository;
    }

    public Task<IQueryable<Offer>> GetOffers(Query query = null)
    {
        IQueryable<Offer> items = offerRepository.Query().Include(i => i.Request).Include(i => i.Supplier);

        if (query != null)
        {
            if (!string.IsNullOrEmpty(query.Expand))
            {
                var propertiesToExpand = query.Expand.Split(',');
                foreach (var p in propertiesToExpand)
                {
                    items = items.Include(p.Trim());
                }
            }

            items = items.ApplyQuery(query);
        }

        return Task.FromResult(items);
    }

    public async Task<List<Offer>> GetOffersList(Query query = null)
    {
        var items = await GetOffers(query);
        return await items.ToListAsync();
    }

    public async Task<Offer?> GetOfferById(int id)
    {
        return await offerRepository.Query().AsNoTracking().Include(i => i.Request).Include(i => i.Supplier).FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<IEnumerable<Offer>> GetOffersByIds(List<int> ids)
    {
        return await offerRepository.Query().AsNoTracking().Where(i => ids.Contains(i.Id)).Include(i => i.Request).Include(i => i.Supplier).ToListAsync();
    }

    public async Task<Offer> CreateOffer(Offer offer)
    {
        await offerRepository.AddAsync(offer);
        await offerRepository.SaveChangesAsync();
        return offer;
    }

    public async Task<Offer> UpdateOffer(int id, Offer offer)
    {
        var itemToUpdate = offerRepository.Query().FirstOrDefault(i => i.Id == offer.Id);
        if (itemToUpdate == null)
        {
            throw new Exception("Item no longer available");
        }

        offerRepository.Update(offer);
        await offerRepository.SaveChangesAsync();
        return offer;
    }

    public async Task<Offer> DeleteOffer(int id)
    {
        var itemToDelete = offerRepository.Query().FirstOrDefault(i => i.Id == id);
        if (itemToDelete == null)
        {
            throw new Exception("Item no longer available");
        }

        offerRepository.Remove(itemToDelete);
        await offerRepository.SaveChangesAsync();
        return itemToDelete;
    }
}
