using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using T3App.Shared;

namespace T3App.API.Repositories
{
    public interface IEmployeeRepository
    {
        Task<IEnumerable<Employee>> GetEmployees();
        Task<Employee> GetEmployee(Guid id);
        Task<Employee> AddEmployee(Employee employee);
        Task<Employee> UpdateEmployee(Employee employee);
        Task<Employee> DeleteEmployee(Guid id);
    }
}
