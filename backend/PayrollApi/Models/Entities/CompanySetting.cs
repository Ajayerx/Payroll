namespace PayrollApi.Models.Entities;

public class CompanySetting
{
    public Guid Id { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Gstin { get; set; }
    public string? Pan { get; set; }
    public string? LogoUrl { get; set; }
    public bool IsDeleted { get; set; } = false;
    public Guid? CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public Guid? UpdatedBy { get; set; }
    public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;
}
