namespace PayrollApi.Utils;

public static class TaxCalculator
{
    public static decimal CalculateIncomeTax(decimal annualGrossSalary)
    {
        return annualGrossSalary switch
        {
            <= 250000 => 0,
            <= 500000 => (annualGrossSalary - 250000) * 0.05m,
            <= 1000000 => 12500 + (annualGrossSalary - 500000) * 0.20m,
            _ => 112500 + (annualGrossSalary - 1000000) * 0.30m
        };
    }

    public static decimal CalculateMonthlyTax(decimal annualGrossSalary)
    {
        return CalculateIncomeTax(annualGrossSalary) / 12;
    }

    public static decimal CalculateEpf(decimal basicSalary)
    {
        // Employee contribution: 12% of basic
        return basicSalary * 0.12m;
    }

    public static decimal CalculateEsi(decimal grossSalary)
    {
        // Employee contribution: 0.75% of gross (for gross <= 21,000)
        if (grossSalary > 21000) return 0;
        return grossSalary * 0.0075m;
    }

    public static decimal CalculateProfessionalTax(decimal grossSalary)
    {
        // Simplified Karnataka PT slab
        return grossSalary switch
        {
            <= 15000 => 0,
            <= 20000 => 150,
            _ => 200
        };
    }
}
