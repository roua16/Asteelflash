using System.Linq;
using System.Linq.Dynamic.Core;
using ITStockM.Data;
using ITStockM.Models.ITStockManagment;
using ITStockM.Repositories;
using ITStockM.Services.Utilities;
using Microsoft.EntityFrameworkCore;
using Radzen;

namespace ITStockM.Services.Employees;

public class EmployeeService : IEmployeeService
{
    private readonly IRepository<Employee> employeeRepository;

    public EmployeeService(IRepository<Employee> employeeRepository)
    {
        this.employeeRepository = employeeRepository;
    }

    public Task<IQueryable<Employee>> GetEmployees(Query query = null)
    {
        IQueryable<Employee> items = employeeRepository.Query();

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

    public async Task<List<Employee>> GetEmployeesList(Query query = null)
    {
        var items = await GetEmployees(query);
        return await items.ToListAsync();
    }

    public async Task<Employee?> GetEmployeeById(int id)
    {
        return await employeeRepository.Query().AsNoTracking().FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<Employee> CreateEmployee(Employee employee)
    {
        await employeeRepository.AddAsync(employee);
        await employeeRepository.SaveChangesAsync();
        return employee;
    }

    public async Task<Employee> UpdateEmployee(int id, Employee employee)
    {
        var itemToUpdate = employeeRepository.Query().FirstOrDefault(i => i.Id == employee.Id);
        if (itemToUpdate == null)
        {
            throw new Exception("Item no longer available");
        }

        employeeRepository.Update(employee);
        await employeeRepository.SaveChangesAsync();
        return employee;
    }

    public async Task<Employee> DeleteEmployee(int id)
    {
        var itemToDelete = employeeRepository.Query().FirstOrDefault(i => i.Id == id);
        if (itemToDelete == null)
        {
            throw new Exception("Item no longer available");
        }

        employeeRepository.Remove(itemToDelete);
        await employeeRepository.SaveChangesAsync();
        return itemToDelete;
    }
}
