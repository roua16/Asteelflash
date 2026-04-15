using ITStockM.Application.Common.Models;
using ITStockM.Domain.Entities;

namespace ITStockM.Services.Offers;

public interface IOfferService
{
    Task<IQueryable<Offer>> GetOffers(QueryOptions? query = null);
    Task<List<Offer>> GetOffersList(QueryOptions? query = null);
    Task<Offer?> GetOfferById(int id);
    Task<IEnumerable<Offer>> GetOffersByIds(List<int> ids);
    Task<Offer> CreateOffer(Offer offer);
    Task<Offer> UpdateOffer(int id, Offer offer);
    Task<Offer> DeleteOffer(int id);
}
