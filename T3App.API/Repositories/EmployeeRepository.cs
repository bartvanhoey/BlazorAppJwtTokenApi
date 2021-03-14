using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using T3App.Shared;

namespace T3App.API.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly AppDbContext appDbContext;

        public EmployeeRepository(AppDbContext appDbContext)
        {
            this.appDbContext = appDbContext;
        }

        public async Task<IEnumerable<Employee>> GetEmployees()
        {
            return await appDbContext.Employees.ToListAsync();
        }

        public async Task<Employee> GetEmployee(Guid id)
        {
            return await appDbContext.Employees.FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<Employee> AddEmployee(Employee employee)
        {
            var result = await appDbContext.Employees.AddAsync(employee);
            await appDbContext.SaveChangesAsync();
            return result.Entity;
        }

        public async Task<Employee> UpdateEmployee(Employee employee)
        {
            var updateEmployee= await appDbContext.Employees.FirstOrDefaultAsync(e => e.Id == employee.Id);
            if (updateEmployee == null) return null;
            updateEmployee.Name = employee.Name;
            await appDbContext.SaveChangesAsync();
            return updateEmployee;
        }

        public async Task<Employee> DeleteEmployee(Guid id)
        {
            var deleteEmployee = await appDbContext.Employees.FirstOrDefaultAsync(e => e.Id == id);
            if (deleteEmployee != null)
            {
                appDbContext.Employees.Remove(deleteEmployee);
                await appDbContext.SaveChangesAsync();
                return deleteEmployee;
            }
            return null;
        }
    }
}
