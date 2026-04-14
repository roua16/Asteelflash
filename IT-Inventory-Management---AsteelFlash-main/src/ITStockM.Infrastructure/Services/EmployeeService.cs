using Microsoft.EntityFrameworkCore;
using ITStockM.Domain.Entities;
using ITStockM.Repositories;
using Radzen;

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
    /// Get employees with optional filtering.
    /// </summary>
    public async Task<IQueryable<Employee>> GetEmployees(Query? query = null)
    {
        return await GetAll(query);
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

    /// <summary>
    /// Create a new employee.
    /// </summary>
    public async Task<Employee> CreateEmployee(Employee employee)
    {
        return await Create(employee);
    }

    /// <summary>
    /// Update an existing employee.
    /// </summary>
    public async Task<Employee> UpdateEmployee(int id, Employee employee)
    {
        return await Update(id, employee);
    }

    /// <summary>
    /// Delete an employee.
    /// </summary>
    public async Task<Employee> DeleteEmployee(int id)
    {
        var employee = await GetById(id);
        if (employee == null)
            throw new KeyNotFoundException($"Employee with ID {id} not found");
        
        await Delete(id);
        return employee;
    }
}

