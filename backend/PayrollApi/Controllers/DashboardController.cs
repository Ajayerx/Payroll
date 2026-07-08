using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayrollApi.Models.DTOs;
using PayrollApi.Services.Interfaces;

namespace PayrollApi.Controllers;

[Route("api/v1/dashboard")]
[Authorize]
public class DashboardController : BaseApiController
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet]
    public async Task<ActionResult<DashboardDto>> GetDashboard([FromQuery] int? month, [FromQuery] int? year)
    {
        var result = await _dashboardService.GetDashboardAsync(month, year);
        return Ok(result);
    }

    [HttpGet("export")]
    [Authorize(Roles = "Admin,HRManager")]
    public async Task<ActionResult> ExportDashboard([FromQuery] int? month, [FromQuery] int? year)
    {
        var data = await _dashboardService.GetDashboardAsync(month, year);

        var csv = new System.Text.StringBuilder();
        csv.AppendLine("Metric,Value");
        csv.AppendLine($"Total Employees,{data.TotalEmployees}");
        csv.AppendLine($"Active Employees,{data.ActiveEmployees}");
        csv.AppendLine($"New Hires This Month,{data.NewHiresThisMonth}");
        csv.AppendLine($"Pending Payroll Amount,{data.PendingPayrollAmount}");
        csv.AppendLine($"Pending Payroll Count,{data.PendingPayrollCount}");
        csv.AppendLine($"Total Deductions,{data.TotalDeductions}");
        csv.AppendLine($"Net Disbursed YTD,{data.NetDisbursedYTD}");

        return File(System.Text.Encoding.UTF8.GetBytes(csv.ToString()),
            "text/csv", $"dashboard_export_{DateTime.UtcNow:yyyyMMdd}.csv");
    }
}
