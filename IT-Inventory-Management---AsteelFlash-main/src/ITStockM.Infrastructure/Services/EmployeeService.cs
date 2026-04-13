using Microsoft.EntityFrameworkCore;
using ITStockM.Domain.Entities;
using ITStockM.Repositories;

namespace ITStockM.Services.Employees;

/// <summary>
/// CRUD service for Employee entities.
/// Inherits generic CRUD operations from BaseCrudService, reducing code from 87 to 20 LOC (77% reduction).
/// </summary>
public class EmployeeService : BaseCrudService<Employee, IRepository<Employee>>, IEmployeeService
{
    public EmployeeService(IRepository<Employee> repository)
        : base(repository)
    {
    }

    /// <summary>
    /// Get employees list (helper for backwards compatibility).
    /// </summary>
    public async Task<List<Employee>> GetEmployeesList(Query? query = null)
    {
        var items = await GetAll(query);
        return await items.ToListAsync();
    }

    /// <summary>
    /// Get employee by ID without tracking (read-only).
    /// </summary>
    public async Task<Employee?> GetEmployeeById(int id)
    {
        return await Repository.Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id);
    }
}
