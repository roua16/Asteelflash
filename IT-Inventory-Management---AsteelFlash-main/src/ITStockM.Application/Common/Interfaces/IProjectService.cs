using ITStockM.Domain.Entities;
using Radzen;

namespace ITStockM.Services.Projects;

public interface IProjectService
{
    Task<IQueryable<Project>> GetProjects(Query query = null);
    Task<List<Project>> GetProjectsList(Query query = null);
    Task<Project?> GetProjectById(int id);
    Task<Project> CreateProject(Project project);
    Task<Project> UpdateProject(int id, Project project);
    Task<Project> DeleteProject(int id);
}
