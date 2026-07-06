using Microsoft.EntityFrameworkCore;
using PayrollApi.Data;
using PayrollApi.Models.DTOs;
using PayrollApi.Models.Entities;
using PayrollApi.Models.Enums;
using PayrollApi.Utils;

namespace PayrollApi.Services;

public interface ISalaryCalculationService
{
    Task<SalaryBreakdownDto> CalculateAsync(Guid employeeId, int month, int year);
    Task<SalaryBreakdownDto> CalculateAnnualAsync(Guid employeeId, int year);
}

public class SalaryCalculationService : ISalaryCalculationService
{
    private readonly PayrollDbContext _context;

    public SalaryCalculationService(PayrollDbContext context)
    {
        _context = context;
    }

    public async Task<SalaryBreakdownDto> CalculateAsync(Guid employeeId, int month, int year)
    {
        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.Id == employeeId && !e.IsDeleted)
            ?? throw new KeyNotFoundException($"Employee {employeeId} not found");

        var structures = await _context.EmployeeSalaryStructures
            .Include(s => s.SalaryComponent)
            .Where(s => s.EmployeeId == employeeId && !s.IsDeleted)
            .ToListAsync();

        var deductions = await _context.Deductions
            .Where(d => d.EmployeeId == employeeId && !d.IsDeleted && d.RemainingAmount > 0)
            .ToListAsync();

        var taxSlabs = await _context.TaxSlabs
            .Where(t => t.IsActive && !t.IsDeleted)
            .OrderBy(t => t.FromAmount)
            .ToListAsync();

        var earnings = structures
            .Where(s => s.SalaryComponent.Type == SalaryComponentType.Earning)
            .ToList();

        var deductionComponents = structures
            .Where(s => s.SalaryComponent.Type == SalaryComponentType.Deduction)
            .ToList();

        var earningsBreakdown = earnings.Select(e => new ComponentBreakdown
        {
            ComponentName = e.SalaryComponent.Name,
            ComponentType = "Earning",
            Amount = e.Amount,
            IsVariable = e.SalaryComponent.IsVariable
        }).ToList();

        var monthlyBasic = earnings
            .FirstOrDefault(e => e.SalaryComponent.Name.Contains("Basic", StringComparison.OrdinalIgnoreCase))
            ?.Amount ?? earnings.Sum(e => e.Amount) * 0.45m;

        var grossEarnings = earnings.Sum(e => e.Amount);
        var annualGross = grossEarnings * 12;

        var pfDeduction = deductionComponents
            .FirstOrDefault(d => d.SalaryComponent.Name.Contains("PF", StringComparison.OrdinalIgnoreCase));

        var epfAmount = pfDeduction?.Amount ?? Math.Min(TaxCalculator.CalculateEpf(monthlyBasic), 1800);

        var esiAmount = deductionComponents
            .FirstOrDefault(d => d.SalaryComponent.Name.Contains("ESI", StringComparison.OrdinalIgnoreCase))
            ?.Amount ?? TaxCalculator.CalculateEsi(grossEarnings);

        var professionalTax = deductionComponents
            .FirstOrDefault(d => d.SalaryComponent.Name.Contains("Professional Tax", StringComparison.OrdinalIgnoreCase))
            ?.Amount ?? TaxCalculator.CalculateProfessionalTax(grossEarnings);

        var monthlyIncomeTax = taxSlabs.Count > 0
            ? CalculateMonthlyTaxFromSlabs(annualGross, taxSlabs)
            : TaxCalculator.CalculateMonthlyTax(annualGross);

        var otherDeductionsAmount = deductionComponents
            .Where(d => !d.SalaryComponent.Name.Contains("PF", StringComparison.OrdinalIgnoreCase)
                     && !d.SalaryComponent.Name.Contains("ESI", StringComparison.OrdinalIgnoreCase)
                     && !d.SalaryComponent.Name.Contains("Professional Tax", StringComparison.OrdinalIgnoreCase)
                     && !d.SalaryComponent.Name.Contains("TDS", StringComparison.OrdinalIgnoreCase)
                     && !d.SalaryComponent.Name.Contains("Income Tax", StringComparison.OrdinalIgnoreCase))
            .Sum(d => d.Amount);

        var loanDeductions = deductions.Sum(d => d.RemainingAmount > 0
            ? Math.Min(d.RemainingAmount, d.Amount * 0.1m)
            : 0);

        var deductionsBreakdown = new List<ComponentBreakdown>
        {
            new() { ComponentName = "EPF", ComponentType = "Deduction", Amount = epfAmount },
            new() { ComponentName = "ESI", ComponentType = "Deduction", Amount = esiAmount },
            new() { ComponentName = "Professional Tax", ComponentType = "Deduction", Amount = professionalTax },
            new() { ComponentName = "Income Tax / TDS", ComponentType = "Deduction", Amount = monthlyIncomeTax }
        };

        if (otherDeductionsAmount > 0)
            deductionsBreakdown.Add(new() { ComponentName = "Other Deductions", ComponentType = "Deduction", Amount = otherDeductionsAmount });

        if (loanDeductions > 0)
            deductionsBreakdown.Add(new() { ComponentName = "Loan/Advance Recovery", ComponentType = "Deduction", Amount = loanDeductions });

        var totalDeductions = deductionsBreakdown.Sum(d => d.Amount);
        var netSalary = grossEarnings - totalDeductions;

        return new SalaryBreakdownDto
        {
            EmployeeId = employeeId,
            EmployeeName = $"{employee.FirstName} {employee.LastName}",
            EmployeeCode = employee.EmployeeCode,
            Month = month,
            Year = year,
            GrossEarnings = grossEarnings,
            TotalDeductions = totalDeductions,
            NetSalary = netSalary,
            Earnings = earningsBreakdown,
            Deductions = deductionsBreakdown
        };
    }

    public async Task<SalaryBreakdownDto> CalculateAnnualAsync(Guid employeeId, int year)
    {
        var monthly = await CalculateAsync(employeeId, 1, year);
        monthly.GrossEarnings *= 12;
        monthly.TotalDeductions *= 12;
        monthly.NetSalary *= 12;
        monthly.Month = 0;
        monthly.Year = year;
        return monthly;
    }

    private static decimal CalculateMonthlyTaxFromSlabs(decimal annualGross, List<TaxSlab> slabs)
    {
        var annualTax = 0m;
        var remaining = annualGross;

        foreach (var slab in slabs.OrderBy(s => s.FromAmount))
        {
            if (remaining <= 0) break;

            var slabAmount = slab.ToAmount.HasValue
                ? Math.Min(remaining, slab.ToAmount.Value - slab.FromAmount)
                : remaining;

            annualTax += slabAmount * slab.Rate / 100;
            remaining -= slabAmount;
        }

        return annualTax / 12;
    }
}

public class SalaryBreakdownDto
{
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
    public int Month { get; set; }
    public int Year { get; set; }
    public decimal GrossEarnings { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal NetSalary { get; set; }
    public List<ComponentBreakdown> Earnings { get; set; } = [];
    public List<ComponentBreakdown> Deductions { get; set; } = [];
}

public class ComponentBreakdown
{
    public string ComponentName { get; set; } = string.Empty;
    public string ComponentType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public bool IsVariable { get; set; }
}
