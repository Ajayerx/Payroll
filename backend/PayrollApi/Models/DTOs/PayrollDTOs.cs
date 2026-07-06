namespace PayrollApi.Models.DTOs;

public class PayrollDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public int Month { get; set; }
    public int Year { get; set; }
    public decimal GrossSalary { get; set; }
    public decimal TaxDeduction { get; set; }
    public decimal OtherDeductions { get; set; }
    public decimal NetSalary { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? ProcessedDate { get; set; }
    public string? Remarks { get; set; }
}

public class PayrollDetailDto
{
    public Guid Id { get; set; }
    public Guid PayrollId { get; set; }
    public string ComponentName { get; set; } = string.Empty;
    public string ComponentType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class ProcessPayrollRequest
{
    public List<Guid> EmployeeIds { get; set; } = [];
    public int Month { get; set; }
    public int Year { get; set; }
    public string? Remarks { get; set; }
}

public class BulkProcessPayrollRequest
{
    public List<Guid> EmployeeIds { get; set; } = [];
    public int Month { get; set; }
    public int Year { get; set; }
}

public class UpdatePayrollRequest
{
    public decimal? TaxDeduction { get; set; }
    public decimal? OtherDeductions { get; set; }
    public string? Status { get; set; }
    public string? Remarks { get; set; }
}

public class PayrollListResponse
{
    public List<PayrollDto> Items { get; set; } = new();
    public int Total { get; set; }
}
