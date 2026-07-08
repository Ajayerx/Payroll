namespace PayrollApi.Models.DTOs;

public class DashboardDto
{
    public int TotalEmployees { get; set; }
    public int ActiveEmployees { get; set; }
    public int NewHiresThisMonth { get; set; }
    public decimal PendingPayrollAmount { get; set; }
    public int PendingPayrollCount { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal NetDisbursedYTD { get; set; }
    public List<MonthlyPayrollTrend> MonthlyTrend { get; set; } = new();
    public List<DepartmentSummaryDto> DepartmentOverview { get; set; } = new();
    public List<RecentPayrollDto> RecentTransactions { get; set; } = new();
}

public class MonthlyPayrollTrend
{
    public string Month { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class RecentPayrollDto
{
    public Guid Id { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public int Month { get; set; }
    public int Year { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? ProcessedDate { get; set; }
}
