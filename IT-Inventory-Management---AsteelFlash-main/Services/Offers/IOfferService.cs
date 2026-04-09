using ITStockM.Models.ITStockManagment;
using Radzen;

namespace ITStockM.Services.Offers;

public interface IOfferService
{
    Task<IQueryable<Offer>> GetOffers(Query query = null);
    Task<List<Offer>> GetOffersList(Query query = null);
    Task<Offer?> GetOfferById(int id);
    Task<IEnumerable<Offer>> GetOffersByIds(List<int> ids);
    Task<Offer> CreateOffer(Offer offer);
    Task<Offer> UpdateOffer(int id, Offer offer);
    Task<Offer> DeleteOffer(int id);
}
