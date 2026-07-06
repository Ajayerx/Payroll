using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayrollApi.Models.DTOs;
using PayrollApi.Services;

namespace PayrollApi.Controllers;

[Route("api/v1")]
[Authorize(Roles = "Admin,HRManager")]
public class SalaryController : BaseApiController
{
    private readonly ISalaryService _salaryService;
    private readonly ISalaryCalculationService _salaryCalculation;

    public SalaryController(ISalaryService salaryService, ISalaryCalculationService salaryCalculation)
    {
        _salaryService = salaryService;
        _salaryCalculation = salaryCalculation;
    }

    [HttpGet("salary-components")]
    public async Task<ActionResult<List<SalaryComponentDto>>> GetComponents()
    {
        var result = await _salaryService.GetComponentsAsync();
        return Ok(result);
    }

    [HttpPost("salary-components")]
    public async Task<ActionResult<SalaryComponentDto>> CreateComponent([FromBody] CreateSalaryComponentRequest request)
    {
        var result = await _salaryService.CreateComponentAsync(request);
        return CreatedAtAction(nameof(GetComponents), null, result);
    }

    [HttpPut("salary-components/{id}")]
    public async Task<ActionResult<SalaryComponentDto>> UpdateComponent(Guid id, [FromBody] UpdateSalaryComponentRequest request)
    {
        try
        {
            var result = await _salaryService.UpdateComponentAsync(id, request);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("employees/{employeeId}/salary-structure")]
    public async Task<ActionResult<List<EmployeeSalaryStructureDto>>> GetEmployeeStructure(Guid employeeId)
    {
        var result = await _salaryService.GetEmployeeStructureAsync(employeeId);
        return Ok(result);
    }

    [HttpPut("employees/{employeeId}/salary-structure")]
    public async Task<ActionResult> UpdateEmployeeStructure(Guid employeeId, [FromBody] UpdateSalaryStructureRequest request)
    {
        await _salaryService.UpdateEmployeeStructureAsync(employeeId, request);
        return Ok(new { message = "Salary structure updated" });
    }

    [HttpGet("employees/{employeeId}/deductions")]
    public async Task<ActionResult<List<DeductionDto>>> GetEmployeeDeductions(Guid employeeId)
    {
        var result = await _salaryService.GetEmployeeDeductionsAsync(employeeId);
        return Ok(result);
    }

    [HttpPost("employees/{employeeId}/deductions")]
    public async Task<ActionResult<DeductionDto>> AddEmployeeDeduction(Guid employeeId, [FromBody] CreateDeductionRequest request)
    {
        var result = await _salaryService.AddEmployeeDeductionAsync(employeeId, request);
        return CreatedAtAction(nameof(GetEmployeeDeductions), new { employeeId }, result);
    }

    [HttpGet("employees/{employeeId}/salary-calculation")]
    public async Task<ActionResult<SalaryBreakdownDto>> PreviewCalculation(Guid employeeId, [FromQuery] int month, [FromQuery] int year)
    {
        try
        {
            var result = await _salaryCalculation.CalculateAsync(employeeId, month, year);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
