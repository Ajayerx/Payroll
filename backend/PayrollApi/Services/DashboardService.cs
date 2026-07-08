using Microsoft.EntityFrameworkCore;
using PayrollApi.Data;
using PayrollApi.Models.DTOs;
using PayrollApi.Models.Enums;
using PayrollApi.Services.Interfaces;

namespace PayrollApi.Services;

public class DashboardService : IDashboardService
{
    private readonly PayrollDbContext _context;

    public DashboardService(PayrollDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardDto> GetDashboardAsync(int? month, int? year)
    {
        var now = DateTime.UtcNow;
        var targetMonth = month ?? now.Month;
        var targetYear = year ?? now.Year;

        var activeEmployees = await _context.Employees.CountAsync(e => e.IsActive && !e.IsDeleted);
        var newHires = await _context.Employees
            .CountAsync(e => e.DateOfJoining.HasValue
                && e.DateOfJoining.Value.Month == targetMonth
                && e.DateOfJoining.Value.Year == targetYear);

        var pendingPayrolls = await _context.Payrolls
            .Where(p => p.PayrollMonth.Month == targetMonth
                && p.PayrollMonth.Year == targetYear
                && p.Status == PayrollStatus.Draft)
            .ToListAsync();
        var pendingAmount = pendingPayrolls.Sum(p => p.NetSalary);
        var pendingCount = pendingPayrolls.Count;

        var totalDeductions = await _context.Payrolls
            .Where(p => p.PayrollMonth.Year == targetYear)
            .SumAsync(p => p.TaxDeduction + p.OtherDeductions);

        var netYtd = await _context.Payrolls
            .Where(p => p.PayrollMonth.Year == targetYear)
            .SumAsync(p => p.NetSalary);

        var monthlyTrend = await _context.PayrollMonths
            .Where(pm => pm.Year == targetYear)
            .OrderBy(pm => pm.Month)
            .Select(pm => new MonthlyPayrollTrend
            {
                Month = pm.Month.ToString(),
                Amount = pm.Payrolls.Sum(p => p.NetSalary)
            })
            .ToListAsync();

        var departmentOverview = await _context.Employees
            .Where(e => e.IsActive && !e.IsDeleted && e.Department != null)
            .GroupBy(e => e.Department)
            .Select(g => new DepartmentSummaryDto
            {
                Department = g.Key!,
                EmployeeCount = g.Count(),
                TotalSalary = 0,
                AverageSalary = 0
            })
            .ToListAsync();

        var recentTransactions = await _context.Payrolls
            .Include(p => p.Employee)
            .Include(p => p.PayrollMonth)
            .OrderByDescending(p => p.ProcessedDate)
            .Take(5)
            .Select(p => new RecentPayrollDto
            {
                Id = p.Id,
                EmployeeName = p.Employee.FirstName + " " + p.Employee.LastName,
                Month = p.PayrollMonth.Month,
                Year = p.PayrollMonth.Year,
                Amount = p.NetSalary,
                Status = p.Status.ToString(),
                ProcessedDate = p.ProcessedDate
            })
            .ToListAsync();

        return new DashboardDto
        {
            TotalEmployees = activeEmployees,
            ActiveEmployees = activeEmployees,
            NewHiresThisMonth = newHires,
            PendingPayrollAmount = pendingAmount,
            PendingPayrollCount = pendingCount,
            TotalDeductions = totalDeductions,
            NetDisbursedYTD = netYtd,
            MonthlyTrend = monthlyTrend,
            DepartmentOverview = departmentOverview,
            RecentTransactions = recentTransactions
        };
    }
}
