using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using T3App.API.Repositories;
using T3App.Shared;

namespace T3App.API.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeRepository _employeeRepository;

        public EmployeesController(IEmployeeRepository employeeRepository)
        {
            _employeeRepository = employeeRepository;
        }

        [HttpGet]
        public async Task<ActionResult> GetEmployees()
        {
            try
            {
                return Ok(await _employeeRepository.GetEmployees());
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<Employee>> GetEmployee(Guid id)
        {
            try
            {
                var result = await _employeeRepository.GetEmployee(id);
                if (result == null) return NotFound();
                return result;
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database");
            }
        }

        [HttpPost]
        public async Task<ActionResult<Employee>> CreateEmployee(Employee employee)
        {
            try
            {
                if (employee == null) return BadRequest();
                var createdEmployee = await _employeeRepository.AddEmployee(employee);
                return CreatedAtAction(nameof(GetEmployee), new { id = createdEmployee.Id }, createdEmployee);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error creating new employee record");
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<Employee>> UpdateEmployee(Guid id, Employee employee)
        {
            try
            {
                if (id != employee.Id) return BadRequest("Employee ID mismatch");
                var employeeToUpdate = await _employeeRepository.GetEmployee(id);
                if (employeeToUpdate == null) return NotFound($"Employee with Id = {id} not found");
                return await _employeeRepository.UpdateEmployee(employee);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error updating data");
            }
        }

        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<Employee>> DeleteEmployee(Guid id)
        {
            try
            {
                var employeeToDelete = await _employeeRepository.GetEmployee(id);
                if (employeeToDelete == null) return NotFound($"Employee with Id = {id} not found");
                return await _employeeRepository.DeleteEmployee(id);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error deleting data");
            }
        }
    }
}
