namespace PayrollApi.Models.DTOs;

public class EmployeeDto
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
    public bool IsActive { get; set; }
}

public class CreateEmployeeRequest
{
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
}

public class UpdateEmployeeRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
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
    public bool? IsActive { get; set; }
}

public class EmployeeListResponse
{
    public List<EmployeeDto> Items { get; set; } = new();
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}
