using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayrollApi.Data;
using PayrollApi.Models.DTOs;
using PayrollApi.Services;

namespace PayrollApi.Controllers;

[Route("api/v1/payroll")]
[Authorize(Roles = "Admin,HRManager")]
public class PayrollController : BaseApiController
{
    private readonly IPayrollService _payrollService;
    private readonly PayrollDbContext _context;

    public PayrollController(IPayrollService payrollService, PayrollDbContext context)
    {
        _payrollService = payrollService;
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<PayrollListResponse>> GetAll(
        [FromQuery] int? month, [FromQuery] int? year,
        [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _payrollService.GetAllAsync(month, year, status, page, pageSize);
        return Ok(result);
    }

    [HttpPost("process")]
    public async Task<ActionResult<PayrollDto>> Process([FromBody] ProcessPayrollRequest request)
    {
        var result = await _payrollService.ProcessAsync(request);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PayrollDto>> GetById(Guid id)
    {
        try
        {
            var result = await _payrollService.GetByIdAsync(id);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<PayrollDto>> Update(Guid id, [FromBody] UpdatePayrollRequest request)
    {
        try
        {
            var result = await _payrollService.UpdateAsync(id, request);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("{id}/salary-slip")]
    public async Task<ActionResult> GetSalarySlip(Guid id)
    {
        try
        {
            var slip = await _payrollService.GetSalarySlipAsync(id);
            return File(slip, "text/plain", $"salary-slip-{id}.txt");
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost("{id}/generate-slip")]
    public async Task<ActionResult> GenerateSlip(Guid id)
    {
        try
        {
            var result = await _payrollService.GenerateSlipAsync(id);
            return Ok(new { message = result });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("month/{month}/year/{year}")]
    public async Task<ActionResult<PayrollListResponse>> GetByMonthYear(int month, int year)
    {
        var result = await _payrollService.GetByMonthYearAsync(month, year);
        return Ok(result);
    }

    [HttpPost("bulk-process")]
    public async Task<ActionResult<List<PayrollDto>>> BulkProcess([FromBody] BulkProcessPayrollRequest request)
    {
        var result = await _payrollService.BulkProcessAsync(request);
        return Ok(result);
    }

    [HttpPut("month/{month}/year/{year}/lock")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> LockPayrollMonth(int month, int year)
    {
        var payrollMonth = await _context.PayrollMonths
            .FirstOrDefaultAsync(pm => pm.Month == month && pm.Year == year);
        if (payrollMonth == null)
            return NotFound(new { message = "Payroll period not found" });
        payrollMonth.IsLocked = true;
        await _context.SaveChangesAsync();
        return Ok(new { message = "Payroll month locked" });
    }

    [HttpPut("month/{month}/year/{year}/unlock")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> UnlockPayrollMonth(int month, int year)
    {
        var payrollMonth = await _context.PayrollMonths
            .FirstOrDefaultAsync(pm => pm.Month == month && pm.Year == year);
        if (payrollMonth == null)
            return NotFound(new { message = "Payroll period not found" });
        payrollMonth.IsLocked = false;
        await _context.SaveChangesAsync();
        return Ok(new { message = "Payroll month unlocked" });
    }

    [HttpGet("export/csv")]
    public async Task<ActionResult> ExportCsv(
        [FromQuery] int? month, [FromQuery] int? year, [FromQuery] string? status)
    {
        var data = await _payrollService.ExportCsvAsync(month, year, status);
        return File(data, "text/csv", $"payroll_export_{DateTime.UtcNow:yyyyMMdd}.csv");
    }

    [HttpGet("{id}/export/pdf")]
    public async Task<ActionResult> ExportPdf(Guid id)
    {
        try
        {
            var data = await _payrollService.ExportPdfAsync(id);
            return File(data, "application/pdf", $"salary_slip_{id}.pdf");
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
