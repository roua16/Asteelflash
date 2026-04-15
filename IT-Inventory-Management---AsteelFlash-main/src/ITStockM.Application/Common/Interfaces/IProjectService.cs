using ITStockM.Application.Common.Models;
using ITStockM.Domain.Entities;

namespace ITStockM.Services.Projects;

public interface IProjectService
{
    Task<IQueryable<Project>> GetProjects(QueryOptions? query = null);
    Task<List<Project>> GetProjectsList(QueryOptions? query = null);
    Task<Project?> GetProjectById(int id);
    Task<Project> CreateProject(Project project);
    Task<Project> UpdateProject(int id, Project project);
    Task<Project> DeleteProject(int id);
}
