using PayrollApi.Models.Enums;

namespace PayrollApi.Models.Entities;

public class PayrollMonth
{
    public Guid Id { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsLocked { get; set; }
    public PayrollStatus Status { get; set; } = PayrollStatus.Draft;
    public bool IsDeleted { get; set; } = false;
    public Guid? CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public Guid? UpdatedBy { get; set; }
    public DateTime? UpdatedDate { get; set; }

    public ICollection<Payroll> Payrolls { get; set; } = new List<Payroll>();
}
