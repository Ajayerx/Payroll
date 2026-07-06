using FluentAssertions;
using PayrollApi.Utils;

namespace PayrollApi.Tests;

public class TaxCalculatorTests
{
    [Theory]
    [InlineData(200000, 0)]
    [InlineData(250000, 0)]
    [InlineData(300000, 2500)]
    [InlineData(500000, 12500)]
    [InlineData(600000, 32500)]
    [InlineData(1000000, 112500)]
    [InlineData(1200000, 172500)]
    [InlineData(1800000, 352500)]
    public void CalculateIncomeTax_ShouldReturnCorrectTax(decimal annualGross, decimal expectedTax)
    {
        var result = TaxCalculator.CalculateIncomeTax(annualGross);
        result.Should().Be(expectedTax);
    }

    [Theory]
    [InlineData(600000, 2708.33)]
    [InlineData(1200000, 14375)]
    public void CalculateMonthlyTax_ShouldReturnCorrectAmount(decimal annualGross, decimal expectedMonthly)
    {
        var result = TaxCalculator.CalculateMonthlyTax(annualGross);
        result.Should().BeApproximately(expectedMonthly, 0.01m);
    }

    [Theory]
    [InlineData(50000, 6000)]
    [InlineData(25000, 3000)]
    [InlineData(0, 0)]
    public void CalculateEpf_ShouldReturn12PercentOfBasic(decimal basic, decimal expectedEpf)
    {
        var result = TaxCalculator.CalculateEpf(basic);
        result.Should().Be(expectedEpf);
    }

    [Theory]
    [InlineData(15000, 112.50)]
    [InlineData(21000, 157.50)]
    [InlineData(25000, 0)]
    public void CalculateEsi_ShouldReturnCorrectAmount(decimal gross, decimal expectedEsi)
    {
        var result = TaxCalculator.CalculateEsi(gross);
        result.Should().Be(expectedEsi);
    }

    [Theory]
    [InlineData(10000, 0)]
    [InlineData(15000, 0)]
    [InlineData(18000, 150)]
    [InlineData(25000, 200)]
    [InlineData(100000, 200)]
    public void CalculateProfessionalTax_ShouldReturnCorrectSlab(decimal gross, decimal expectedPt)
    {
        var result = TaxCalculator.CalculateProfessionalTax(gross);
        result.Should().Be(expectedPt);
    }

    [Fact]
    public void CalculateIncomeTax_WithZeroSalary_ShouldReturnZero()
    {
        TaxCalculator.CalculateIncomeTax(0).Should().Be(0);
    }

    [Fact]
    public void CalculateIncomeTax_WithVeryHighSalary_ShouldReturnCorrectAmount()
    {
        var tax = TaxCalculator.CalculateIncomeTax(10_000_000);
        tax.Should().Be(2_812_500);
    }
}
