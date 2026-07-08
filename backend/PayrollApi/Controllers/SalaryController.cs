using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayrollApi.Data;
using PayrollApi.Models.DTOs;
using PayrollApi.Models.Entities;
using PayrollApi.Services;

namespace PayrollApi.Controllers;

[Route("api/v1/salary")]
[Authorize(Roles = "Admin,HRManager")]
public class SalaryController : BaseApiController
{
    private readonly ISalaryService _salaryService;
    private readonly ISalaryCalculationService _salaryCalculation;
    private readonly PayrollDbContext _context;

    public SalaryController(ISalaryService salaryService, ISalaryCalculationService salaryCalculation, PayrollDbContext context)
    {
        _salaryService = salaryService;
        _salaryCalculation = salaryCalculation;
        _context = context;
    }

    [HttpGet("components")]
    public async Task<ActionResult<List<SalaryComponentDto>>> GetComponents()
    {
        var result = await _salaryService.GetComponentsAsync();
        return Ok(result);
    }

    [HttpPost("components")]
    public async Task<ActionResult<SalaryComponentDto>> CreateComponent([FromBody] CreateSalaryComponentRequest request)
    {
        var result = await _salaryService.CreateComponentAsync(request);
        return CreatedAtAction(nameof(GetComponents), null, result);
    }

    [HttpPut("components/{id}")]
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

    [HttpGet("employees/{employeeId}/structure")]
    public async Task<ActionResult<List<EmployeeSalaryStructureDto>>> GetEmployeeStructure(Guid employeeId)
    {
        var result = await _salaryService.GetEmployeeStructureAsync(employeeId);
        return Ok(result);
    }

    [HttpPut("employees/{employeeId}/structure")]
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
    public async Task<ActionResult<DeductionDto>> AddDeduction(Guid employeeId, [FromBody] CreateDeductionRequest request)
    {
        var result = await _salaryService.AddEmployeeDeductionAsync(employeeId, request);
        return Ok(result);
    }

    [HttpPut("employees/{employeeId}/deductions/{deductionId}")]
    public async Task<ActionResult<DeductionDto>> UpdateDeduction(Guid employeeId, Guid deductionId, [FromBody] CreateDeductionRequest request)
    {
        try
        {
            var result = await _salaryService.UpdateEmployeeDeductionAsync(employeeId, deductionId, request);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpDelete("employees/{employeeId}/deductions/{deductionId}")]
    public async Task<ActionResult> DeleteDeduction(Guid employeeId, Guid deductionId)
    {
        try
        {
            await _salaryService.DeleteEmployeeDeductionAsync(employeeId, deductionId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("employees/{employeeId}/tax-config")]
    public async Task<ActionResult<List<TaxConfigurationDto>>> GetTaxConfigurations(Guid employeeId)
    {
        var configs = await _context.TaxConfigurations
            .Where(tc => tc.EmployeeId == employeeId)
            .Select(tc => new TaxConfigurationDto
            {
                Id = tc.Id,
                EmployeeId = tc.EmployeeId,
                TaxSlab = tc.TaxSlab,
                TaxRate = tc.TaxRate,
                EffectiveDate = tc.EffectiveDate
            })
            .ToListAsync();
        return Ok(configs);
    }

    [HttpPost("employees/{employeeId}/tax-config")]
    public async Task<ActionResult<TaxConfigurationDto>> CreateTaxConfiguration(Guid employeeId, [FromBody] CreateTaxConfigurationRequest request)
    {
        var config = new TaxConfiguration
        {
            EmployeeId = employeeId,
            TaxSlab = request.TaxSlab,
            TaxRate = request.TaxRate,
            EffectiveDate = request.EffectiveDate
        };
        _context.TaxConfigurations.Add(config);
        await _context.SaveChangesAsync();
        return Ok(new TaxConfigurationDto
        {
            Id = config.Id,
            EmployeeId = config.EmployeeId,
            TaxSlab = config.TaxSlab,
            TaxRate = config.TaxRate,
            EffectiveDate = config.EffectiveDate
        });
    }

    [HttpDelete("employees/{employeeId}/tax-config/{configId}")]
    public async Task<ActionResult> DeleteTaxConfiguration(Guid employeeId, Guid configId)
    {
        var config = await _context.TaxConfigurations
            .FirstOrDefaultAsync(tc => tc.Id == configId && tc.EmployeeId == employeeId);
        if (config == null) return NotFound(new { message = "Tax configuration not found" });
        _context.TaxConfigurations.Remove(config);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("employees/{employeeId}/calculation")]
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
