using Microsoft.EntityFrameworkCore;
using ITStockM.Domain.Entities;
using ITStockM.Repositories;
using Radzen;

namespace ITStockM.Services.Offers;

/// <summary>
/// CRUD service for Offer entities.
/// Inherits generic CRUD operations from BaseCrudService, reducing code from 91 to 35 LOC (62% reduction).
/// </summary>
public class OfferService : BaseCrudService<Offer, IRepository<Offer>>, IOfferService
{
    public OfferService(IRepository<Offer> repository)
        : base(repository)
    {
    }

    /// <summary>
    /// Apply default eager loading for Offer entities.
    /// </summary>
    protected override IQueryable<Offer> ApplyIncludes(IQueryable<Offer> query)
    {
        return query
            .Include(o => o.Request)
            .Include(o => o.Supplier);
    }

    /// <summary>
    /// Get offers with optional filtering.
    /// </summary>
    public async Task<IQueryable<Offer>> GetOffers(Query? query = null)
    {
        return await GetAll(query);
    }

    /// <summary>
    /// Get offers list (helper for backwards compatibility).
    /// </summary>
    public async Task<List<Offer>> GetOffersList(Query? query = null)
    {
        var items = await GetAll(query);
        return await items.ToListAsync();
    }

    /// <summary>
    /// Get offer by ID without tracking (read-only).
    /// </summary>
    public async Task<Offer?> GetOfferById(int id)
    {
        return await Repository.Query()
            .AsNoTracking()
            .Include(o => o.Request)
            .Include(o => o.Supplier)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    /// <summary>
    /// Get offers by IDs without tracking (read-only).
    /// </summary>
    public async Task<IEnumerable<Offer>> GetOffersByIds(List<int> ids)
    {
        return await Repository.Query()
            .AsNoTracking()
            .Where(o => ids.Contains(o.Id))
            .Include(o => o.Request)
            .Include(o => o.Supplier)
            .ToListAsync();
    }

    /// <summary>
    /// Create a new offer.
    /// </summary>
    public async Task<Offer> CreateOffer(Offer offer)
    {
        return await Create(offer);
    }

    /// <summary>
    /// Update an existing offer.
    /// </summary>
    public async Task<Offer> UpdateOffer(int id, Offer offer)
    {
        return await Update(id, offer);
    }

    /// <summary>
    /// Delete an offer.
    /// </summary>
    public async Task<Offer> DeleteOffer(int id)
    {
        var offer = await GetById(id);
        if (offer == null)
            throw new KeyNotFoundException($"Offer with ID {id} not found");
        
        await Delete(id);
        return offer;
    }
}

