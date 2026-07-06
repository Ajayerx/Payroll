namespace PayrollApi.Models.Entities;

public class PayrollDetail
{
    public Guid Id { get; set; }
    public Guid PayrollId { get; set; }
    public Guid SalaryComponentId { get; set; }
    public decimal Amount { get; set; }
    public bool IsDeleted { get; set; } = false;
    public Guid? CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public Guid? UpdatedBy { get; set; }
    public DateTime? UpdatedDate { get; set; }

    public Payroll Payroll { get; set; } = null!;
    public SalaryComponent SalaryComponent { get; set; } = null!;
}
