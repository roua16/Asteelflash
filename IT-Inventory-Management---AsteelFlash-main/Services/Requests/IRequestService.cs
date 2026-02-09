using System.Linq;
using ITStockM.Models.ITStockManagment;
using Radzen;

namespace ITStockM.Services.Requests;

public interface IRequestService
{
    Task<IQueryable<Request>> GetRequests(Query query = null);
    Task<List<Request>> GetRequestsList(Query query = null);
    Task<Request?> GetRequestById(int id);
    Task<Request> CreateRequest(Request request);
    Task<Request> UpdateRequest(int id, Request request);
    Task<Request> DeleteRequest(int id);
}
