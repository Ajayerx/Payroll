namespace PayrollApi.Models.DTOs;

public class SalaryRegisterRequest
{
    public int? Month { get; set; }
    public int? Year { get; set; }
    public string? Department { get; set; }
}

public class TaxSummaryRequest
{
    public int? Year { get; set; }
}

public class EmployeeEarningsRequest
{
    public Guid? EmployeeId { get; set; }
    public int? Month { get; set; }
    public int? Year { get; set; }
}

public class DepartmentSummaryRequest
{
    public int? Month { get; set; }
    public int? Year { get; set; }
}

public class ExportRequest
{
    public int? Month { get; set; }
    public int? Year { get; set; }
    public string? Department { get; set; }
    public string? ReportType { get; set; }
}

public class SalaryRegisterDto
{
    public string EmployeeCode { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Designation { get; set; } = string.Empty;
    public decimal Basic { get; set; }
    public decimal HRA { get; set; }
    public decimal DA { get; set; }
    public decimal GrossPay { get; set; }
    public decimal Tax { get; set; }
    public decimal OtherDeductions { get; set; }
    public decimal NetPay { get; set; }
}

public class DepartmentSummaryDto
{
    public string Department { get; set; } = string.Empty;
    public int EmployeeCount { get; set; }
    public decimal TotalSalary { get; set; }
    public decimal AverageSalary { get; set; }
}
