namespace PayrollApi.Models.DTOs;

public class SalaryComponentDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool IsVariable { get; set; }
    public string? Description { get; set; }
}

public class CreateSalaryComponentRequest
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool IsVariable { get; set; }
    public string? Description { get; set; }
}

public class UpdateSalaryComponentRequest
{
    public string? Name { get; set; }
    public string? Type { get; set; }
    public bool? IsVariable { get; set; }
    public string? Description { get; set; }
}

public class EmployeeSalaryStructureDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public Guid SalaryComponentId { get; set; }
    public string ComponentName { get; set; } = string.Empty;
    public string ComponentType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime EffectiveDate { get; set; }
}

public class UpdateSalaryStructureRequest
{
    public List<SalaryComponentAmount> Components { get; set; } = new();
}

public class SalaryComponentAmount
{
    public Guid SalaryComponentId { get; set; }
    public decimal Amount { get; set; }
}

public class DeductionDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string Type { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal RemainingAmount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class CreateDeductionRequest
{
    public string Type { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
