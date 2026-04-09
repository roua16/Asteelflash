using System.Linq;
using System.Linq.Dynamic.Core;
using ITStockM.Data;
using ITStockM.Models.ITStockManagment;
using ITStockM.Repositories;
using ITStockM.Services.Utilities;
using Microsoft.EntityFrameworkCore;
using Radzen;

namespace ITStockM.Services.Projects;

public class ProjectService : IProjectService
{
    private readonly IRepository<Project> projectRepository;

    public ProjectService(IRepository<Project> projectRepository)
    {
        this.projectRepository = projectRepository;
    }

    public Task<IQueryable<Project>> GetProjects(Query query = null)
    {
        IQueryable<Project> items = projectRepository.Query();

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

    public async Task<List<Project>> GetProjectsList(Query query = null)
    {
        var items = await GetProjects(query);
        return await items.ToListAsync();
    }

    public async Task<Project?> GetProjectById(int id)
    {
        return await projectRepository.Query().AsNoTracking().FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<Project> CreateProject(Project project)
    {
        await projectRepository.AddAsync(project);
        await projectRepository.SaveChangesAsync();
        return project;
    }

    public async Task<Project> UpdateProject(int id, Project project)
    {
        var itemToUpdate = projectRepository.Query().FirstOrDefault(i => i.Id == project.Id);
        if (itemToUpdate == null)
        {
            throw new Exception("Item no longer available");
        }

        projectRepository.Update(project);
        await projectRepository.SaveChangesAsync();
        return project;
    }

    public async Task<Project> DeleteProject(int id)
    {
        var itemToDelete = projectRepository.Query().FirstOrDefault(i => i.Id == id);
        if (itemToDelete == null)
        {
            throw new Exception("Item no longer available");
        }

        projectRepository.Remove(itemToDelete);
        await projectRepository.SaveChangesAsync();
        return itemToDelete;
    }
}
