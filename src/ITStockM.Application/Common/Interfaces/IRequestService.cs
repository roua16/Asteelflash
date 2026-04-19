using System.Linq;
using ITStockM.Application.Common.Models;
using ITStockM.Domain.Entities;

namespace ITStockM.Services.Requests;

public interface IRequestService
{
    Task<IQueryable<Request>> GetRequests(QueryOptions? query = null);
    Task<List<Request>> GetRequestsList(QueryOptions? query = null);
    Task<Request?> GetRequestById(int id);
    Task<Request> CreateRequest(Request request);
    Task<Request> UpdateRequest(int id, Request request);
    Task<Request> DeleteRequest(int id);
}
