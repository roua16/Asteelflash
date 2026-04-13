using Microsoft.EntityFrameworkCore;
using ITStockM.Domain.Entities;
using ITStockM.Repositories;

namespace ITStockM.Services.Projects;

/// <summary>
/// CRUD service for Project entities.
/// Inherits generic CRUD operations from BaseCrudService, reducing code from 86 to 20 LOC (77% reduction).
/// </summary>
public class ProjectService : BaseCrudService<Project, IRepository<Project>>, IProjectService
{
    public ProjectService(IRepository<Project> repository)
        : base(repository)
    {
    }

    /// <summary>
    /// Get projects list (helper for backwards compatibility).
    /// </summary>
    public async Task<List<Project>> GetProjectsList(Query? query = null)
    {
        var items = await GetAll(query);
        return await items.ToListAsync();
    }

    /// <summary>
    /// Get project by ID without tracking (read-only).
    /// </summary>
    public async Task<Project?> GetProjectById(int id)
    {
        return await Repository.Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);
    }
}
