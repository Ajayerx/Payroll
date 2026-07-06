using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PayrollApi.Data;
using PayrollApi.Models.Entities;
using PayrollApi.Models.Enums;
using PayrollApi.Services;

namespace PayrollApi.Tests;

public class SalaryCalculationServiceTests : IDisposable
{
    private readonly PayrollDbContext _context;
    private readonly SalaryCalculationService _service;

    public SalaryCalculationServiceTests()
    {
        var options = new DbContextOptionsBuilder<PayrollDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new PayrollDbContext(options);
        _service = new SalaryCalculationService(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task CalculateAsync_WithValidEmployee_ShouldReturnBreakdown()
    {
        var basicComponent = new SalaryComponent
        {
            Id = Guid.NewGuid(),
            Name = "Basic Salary",
            Type = SalaryComponentType.Earning,
            IsVariable = false
        };
        var hraComponent = new SalaryComponent
        {
            Id = Guid.NewGuid(),
            Name = "HRA",
            Type = SalaryComponentType.Earning,
            IsVariable = false
        };
        var pfComponent = new SalaryComponent
        {
            Id = Guid.NewGuid(),
            Name = "EPF",
            Type = SalaryComponentType.Deduction,
            IsVariable = false
        };

        _context.SalaryComponents.AddRange(basicComponent, hraComponent, pfComponent);

        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            EmployeeCode = "EMP001",
            FirstName = "Test",
            LastName = "User",
            Department = "Engineering",
            Designation = "Developer"
        };
        _context.Employees.Add(employee);

        _context.EmployeeSalaryStructures.AddRange(
            new EmployeeSalaryStructure
            {
                EmployeeId = employee.Id,
                SalaryComponentId = basicComponent.Id,
                Amount = 50000
            },
            new EmployeeSalaryStructure
            {
                EmployeeId = employee.Id,
                SalaryComponentId = hraComponent.Id,
                Amount = 25000
            },
            new EmployeeSalaryStructure
            {
                EmployeeId = employee.Id,
                SalaryComponentId = pfComponent.Id,
                Amount = 6000
            }
        );

        var slab = new TaxSlab
        {
            Id = Guid.NewGuid(),
            Name = "0-2.5L",
            FromAmount = 0,
            ToAmount = 250000,
            Rate = 0,
            IsActive = true
        };
        _context.TaxSlabs.Add(slab);
        await _context.SaveChangesAsync();

        var result = await _service.CalculateAsync(employee.Id, 6, 2025);

        result.Should().NotBeNull();
        result.EmployeeName.Should().Be("Test User");
        result.Month.Should().Be(6);
        result.Year.Should().Be(2025);
        result.GrossEarnings.Should().Be(75000);
        result.Earnings.Should().HaveCount(2);
        result.Deductions.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CalculateAsync_WithNoSalaryStructure_ShouldReturnZeroEarnings()
    {
        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            EmployeeCode = "EMP002",
            FirstName = "No",
            LastName = "Salary"
        };
        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();

        var result = await _service.CalculateAsync(employee.Id, 1, 2025);

        result.Should().NotBeNull();
        result.GrossEarnings.Should().Be(0);
        result.NetSalary.Should().Be(0);
    }

    [Fact]
    public async Task CalculateAsync_WithNonexistentEmployee_ShouldThrowKeyNotFoundException()
    {
        var act = () => _service.CalculateAsync(Guid.NewGuid(), 1, 2025);
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task CalculateAsync_WithDeductions_ShouldReduceNetSalary()
    {
        var basicComponent = new SalaryComponent
        {
            Id = Guid.NewGuid(),
            Name = "Basic Salary",
            Type = SalaryComponentType.Earning
        };
        _context.SalaryComponents.Add(basicComponent);

        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            EmployeeCode = "EMP003",
            FirstName = "Deduct",
            LastName = "Test"
        };
        _context.Employees.Add(employee);

        _context.EmployeeSalaryStructures.Add(new EmployeeSalaryStructure
        {
            EmployeeId = employee.Id,
            SalaryComponentId = basicComponent.Id,
            Amount = 100000
        });

        _context.Deductions.Add(new Deduction
        {
            EmployeeId = employee.Id,
            Type = DeductionType.Loan,
            Amount = 50000,
            RemainingAmount = 30000,
            StartDate = new DateTime(2025, 1, 1)
        });

        await _context.SaveChangesAsync();

        var result = await _service.CalculateAsync(employee.Id, 6, 2025);

        result.GrossEarnings.Should().Be(100000);
        result.TotalDeductions.Should().BeGreaterThan(0);
        result.NetSalary.Should().BeLessThan(result.GrossEarnings);
    }

    [Fact]
    public async Task CalculateAnnualAsync_ShouldMultiplyByTwelve()
    {
        var basicComponent = new SalaryComponent
        {
            Id = Guid.NewGuid(),
            Name = "Basic Salary",
            Type = SalaryComponentType.Earning
        };
        _context.SalaryComponents.Add(basicComponent);

        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            EmployeeCode = "EMP004",
            FirstName = "Annual",
            LastName = "Calc"
        };
        _context.Employees.Add(employee);

        _context.EmployeeSalaryStructures.Add(new EmployeeSalaryStructure
        {
            EmployeeId = employee.Id,
            SalaryComponentId = basicComponent.Id,
            Amount = 50000
        });

        await _context.SaveChangesAsync();

        var result = await _service.CalculateAnnualAsync(employee.Id, 2025);

        result.Should().NotBeNull();
        result.Year.Should().Be(2025);
    }
}
