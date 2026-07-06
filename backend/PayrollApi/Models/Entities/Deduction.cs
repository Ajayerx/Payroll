using PayrollApi.Models.Enums;

namespace PayrollApi.Models.Entities;

public class Deduction
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public DeductionType Type { get; set; }
    public decimal Amount { get; set; }
    public decimal RemainingAmount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsDeleted { get; set; } = false;
    public Guid? CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public Guid? UpdatedBy { get; set; }
    public DateTime? UpdatedDate { get; set; }

    public Employee Employee { get; set; } = null!;
}
