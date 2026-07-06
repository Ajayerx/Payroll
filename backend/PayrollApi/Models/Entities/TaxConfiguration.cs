namespace PayrollApi.Models.Entities;

public class TaxConfiguration
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string TaxSlab { get; set; } = string.Empty;
    public decimal TaxRate { get; set; }
    public DateTime EffectiveDate { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; } = false;
    public Guid? CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public Guid? UpdatedBy { get; set; }
    public DateTime? UpdatedDate { get; set; }

    public Employee Employee { get; set; } = null!;
}
