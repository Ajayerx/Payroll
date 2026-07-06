namespace PayrollApi.Models.Entities;

public class EmployeeSalaryStructure
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public Guid SalaryComponentId { get; set; }
    public decimal Amount { get; set; }
    public DateTime EffectiveDate { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; } = false;
    public Guid? CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public Guid? UpdatedBy { get; set; }
    public DateTime? UpdatedDate { get; set; }

    public Employee Employee { get; set; } = null!;
    public SalaryComponent SalaryComponent { get; set; } = null!;
}
