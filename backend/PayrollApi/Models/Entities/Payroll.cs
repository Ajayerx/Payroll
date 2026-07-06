using PayrollApi.Models.Enums;

namespace PayrollApi.Models.Entities;

public class Payroll
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public Guid PayrollMonthId { get; set; }
    public decimal GrossSalary { get; set; }
    public decimal TaxDeduction { get; set; }
    public decimal OtherDeductions { get; set; }
    public decimal NetSalary { get; set; }
    public PayrollStatus Status { get; set; } = PayrollStatus.Draft;
    public DateTime? ProcessedDate { get; set; }
    public string? Remarks { get; set; }
    public bool IsDeleted { get; set; } = false;
    public Guid? CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public Guid? UpdatedBy { get; set; }
    public DateTime? UpdatedDate { get; set; }

    public Employee Employee { get; set; } = null!;
    public PayrollMonth PayrollMonth { get; set; } = null!;
    public ICollection<PayrollDetail> PayrollDetails { get; set; } = new List<PayrollDetail>();
}
