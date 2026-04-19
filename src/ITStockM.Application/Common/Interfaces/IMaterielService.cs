using System.Linq;
using ITStockM.Application.Common.Models;
using ITStockM.Domain.Entities;

namespace ITStockM.Services.Materiels;

public interface IMaterielService
{
    Task<IQueryable<Materiel>> GetMateriels(QueryOptions? query = null);
    Task<Materiel?> GetMaterielById(int id);
    Task<Materiel?> GetMaterielByName(string name);
    Task<Materiel> CreateMateriel(Materiel materiel);
    Task<Materiel> UpdateMateriel(int id, Materiel materiel);
    Task<Materiel> DeleteMateriel(int id);
}
