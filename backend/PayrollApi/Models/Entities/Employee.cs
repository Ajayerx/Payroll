namespace PayrollApi.Models.Entities;

public class Employee
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime? DOB { get; set; }
    public string? Gender { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Pincode { get; set; }
    public DateTime? DateOfJoining { get; set; }
    public string? Department { get; set; }
    public string? Designation { get; set; }
    public string? BankName { get; set; }
    public string? BankAccount { get; set; }
    public string? IfscCode { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;
    public Guid? CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public Guid? UpdatedBy { get; set; }
    public DateTime? UpdatedDate { get; set; }

    public User User { get; set; } = null!;
    public ICollection<EmployeeSalaryStructure> SalaryStructures { get; set; } = new List<EmployeeSalaryStructure>();
    public ICollection<Payroll> Payrolls { get; set; } = new List<Payroll>();
    public ICollection<Deduction> Deductions { get; set; } = new List<Deduction>();
    public ICollection<TaxConfiguration> TaxConfigurations { get; set; } = new List<TaxConfiguration>();
}
