using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayrollApi.Constants;
using PayrollApi.Models.DTOs;
using PayrollApi.Services;

namespace PayrollApi.Controllers;

[Route("api/v1/employees")]
[Authorize]
public class EmployeesController : BaseApiController
{
    private readonly IEmployeeService _employeeService;

    public EmployeesController(IEmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,HRManager")]
    public async Task<ActionResult<EmployeeListResponse>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? department = null,
        [FromQuery] string? status = null)
    {
        var result = await _employeeService.GetAllAsync(page, pageSize, search, department, status);
        return Ok(result);
    }

    [HttpGet("me")]
    public async Task<ActionResult<EmployeeDto>> GetCurrentEmployee()
    {
        try
        {
            var result = await _employeeService.GetByUserIdAsync(CurrentUserId);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,HRManager")]
    public async Task<ActionResult<EmployeeDto>> GetById(Guid id)
    {
        try
        {
            var result = await _employeeService.GetByIdAsync(id);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin,HRManager")]
    public async Task<ActionResult<EmployeeDto>> Create([FromBody] CreateEmployeeRequest request)
    {
        var result = await _employeeService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,HRManager")]
    public async Task<ActionResult<EmployeeDto>> Update(Guid id, [FromBody] UpdateEmployeeRequest request)
    {
        try
        {
            var result = await _employeeService.UpdateAsync(id, request);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,HRManager")]
    public async Task<ActionResult> Delete(Guid id)
    {
        try
        {
            await _employeeService.DeleteAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("search")]
    [Authorize(Roles = "Admin,HRManager")]
    public async Task<ActionResult<EmployeeListResponse>> Search([FromQuery] string query)
    {
        var result = await _employeeService.SearchAsync(query);
        return Ok(result);
    }
}
