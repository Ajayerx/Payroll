using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayrollApi.Models.DTOs;
using PayrollApi.Services;

namespace PayrollApi.Controllers;

[Route("api/v1/reports")]
[Authorize(Roles = "Admin,HRManager")]
public class ReportsController : BaseApiController
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("salary-register")]
    public async Task<ActionResult<List<SalaryRegisterDto>>> GetSalaryRegister([FromQuery] SalaryRegisterRequest request)
    {
        var result = await _reportService.GetSalaryRegisterAsync(request);
        return Ok(result);
    }

    [HttpGet("tax-summary")]
    public async Task<ActionResult> GetTaxSummary([FromQuery] TaxSummaryRequest request)
    {
        var result = await _reportService.GetTaxSummaryAsync(request);
        return Ok(result);
    }

    [HttpGet("employee-earnings")]
    public async Task<ActionResult<List<SalaryRegisterDto>>> GetEmployeeEarnings([FromQuery] EmployeeEarningsRequest request)
    {
        var result = await _reportService.GetEmployeeEarningsAsync(request);
        return Ok(result);
    }

    [HttpGet("department-summary")]
    public async Task<ActionResult<List<DepartmentSummaryDto>>> GetDepartmentSummary([FromQuery] DepartmentSummaryRequest request)
    {
        var result = await _reportService.GetDepartmentSummaryAsync(request);
        return Ok(result);
    }

    [HttpPost("export")]
    public async Task<ActionResult> Export([FromBody] ExportRequest request, [FromQuery] string format = "csv")
    {
        var data = await _reportService.ExportAsync(request, format);
        var (contentType, extension) = format.ToLower() switch
        {
            "pdf" => ("text/html", "html"),
            "excel" => ("application/vnd.ms-excel", "xls"),
            _ => ("text/csv", "csv")
        };
        var fileName = $"payroll_report_{DateTime.UtcNow:yyyyMMdd}.{extension}";
        return File(data, contentType, fileName);
    }
}
