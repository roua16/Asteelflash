using ITStockM.Models.ITStockManagment;
using Radzen;

namespace ITStockM.Services.Employees;

public interface IEmployeeService
{
    Task<IQueryable<Employee>> GetEmployees(Query query = null);
    Task<List<Employee>> GetEmployeesList(Query query = null);
    Task<Employee?> GetEmployeeById(int id);
    Task<Employee> CreateEmployee(Employee employee);
    Task<Employee> UpdateEmployee(int id, Employee employee);
    Task<Employee> DeleteEmployee(int id);
}
