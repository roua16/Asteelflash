using ITStockM.Application.Common.Models;
using ITStockM.Domain.Entities;

namespace ITStockM.Services.Employees;

public interface IEmployeeService
{
    Task<IQueryable<Employee>> GetEmployees(QueryOptions? query = null);
    Task<List<Employee>> GetEmployeesList(QueryOptions? query = null);
    Task<Employee?> GetEmployeeById(int id);
    Task<Employee> CreateEmployee(Employee employee);
    Task<Employee> UpdateEmployee(int id, Employee employee);
    Task<Employee> DeleteEmployee(int id);
}
