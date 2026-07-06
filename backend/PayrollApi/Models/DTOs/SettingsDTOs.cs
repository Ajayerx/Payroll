namespace PayrollApi.Models.DTOs;

public class CompanySettingDto
{
    public Guid Id { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Gstin { get; set; }
    public string? Pan { get; set; }
    public string? LogoUrl { get; set; }
}

public class UpdateCompanySettingRequest
{
    public string? CompanyName { get; set; }
    public string? Address { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Gstin { get; set; }
    public string? Pan { get; set; }
}

public class TaxSlabDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal FromAmount { get; set; }
    public decimal? ToAmount { get; set; }
    public decimal Rate { get; set; }
    public DateTime EffectiveDate { get; set; }
}

public class CreateTaxSlabRequest
{
    public string Name { get; set; } = string.Empty;
    public decimal FromAmount { get; set; }
    public decimal? ToAmount { get; set; }
    public decimal Rate { get; set; }
}

public class LeaveTypeDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? DaysPerYear { get; set; }
    public bool IsPaid { get; set; }
}

public class CreateLeaveTypeRequest
{
    public string Name { get; set; } = string.Empty;
    public int? DaysPerYear { get; set; }
    public bool IsPaid { get; set; }
}
