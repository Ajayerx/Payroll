using PayrollApi.Models.Enums;

namespace PayrollApi.Models.Entities;

public class SalaryComponent
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public SalaryComponentType Type { get; set; }
    public bool IsVariable { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;
    public Guid? CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public Guid? UpdatedBy { get; set; }
    public DateTime? UpdatedDate { get; set; }

    public ICollection<EmployeeSalaryStructure> EmployeeStructures { get; set; } = new List<EmployeeSalaryStructure>();
}
