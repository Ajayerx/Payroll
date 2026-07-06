using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using PayrollApi.Data;
using PayrollApi.Models.DTOs;
using PayrollApi.Models.Entities;
using PayrollApi.Models.Enums;
using PayrollApi.Services;

namespace PayrollApi.Tests;

public class PayrollServiceTests : IDisposable
{
    private readonly PayrollDbContext _context;
    private readonly Mock<ISalaryCalculationService> _mockCalc;
    private readonly PayrollService _service;

    public PayrollServiceTests()
    {
        var options = new DbContextOptionsBuilder<PayrollDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new PayrollDbContext(options);
        _mockCalc = new Mock<ISalaryCalculationService>();
        _service = new PayrollService(_context, _mockCalc.Object);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task GetAllAsync_WithNoPayrolls_ShouldReturnEmptyList()
    {
        var result = await _service.GetAllAsync(null, null, null, 1, 20);
        result.Items.Should().BeEmpty();
        result.Total.Should().Be(0);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonexistentId_ShouldThrowKeyNotFoundException()
    {
        var act = () => _service.GetByIdAsync(Guid.NewGuid());
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task ProcessAsync_WithValidRequest_ShouldCreatePayroll()
    {
        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            EmployeeCode = "EMP001",
            FirstName = "Test",
            LastName = "User",
            Department = "Engineering",
            IsActive = true
        };
        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();

        _mockCalc.Setup(c => c.CalculateAsync(employee.Id, 6, 2025))
            .ReturnsAsync(new SalaryBreakdownDto
            {
                EmployeeId = employee.Id,
                EmployeeName = "Test User",
                GrossEarnings = 75000,
                TotalDeductions = 15000,
                NetSalary = 60000,
                Earnings = [new ComponentBreakdown { ComponentName = "Basic", ComponentType = "Earning", Amount = 50000 }],
                Deductions = [new ComponentBreakdown { ComponentName = "PF", ComponentType = "Deduction", Amount = 6000 }]
            });

        var request = new ProcessPayrollRequest
        {
            EmployeeIds = [employee.Id],
            Month = 6,
            Year = 2025
        };

        var result = await _service.ProcessAsync(request);

        result.Should().NotBeNull();
        result.Status.Should().Be(PayrollStatus.Draft.ToString());

        var savedPayroll = await _context.Payrolls.FirstOrDefaultAsync();
        savedPayroll.Should().NotBeNull();
        savedPayroll!.GrossSalary.Should().Be(75000);
        savedPayroll.NetSalary.Should().Be(60000);
    }

    [Fact]
    public async Task ProcessAsync_DuplicatePayroll_ShouldSkip()
    {
        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            EmployeeCode = "EMP002",
            FirstName = "Duplicate",
            LastName = "Test",
            IsActive = true
        };
        _context.Employees.Add(employee);

        var payMonth = new PayrollMonth
        {
            Id = Guid.NewGuid(),
            Month = 6,
            Year = 2025,
            StartDate = new DateTime(2025, 6, 1),
            EndDate = new DateTime(2025, 6, 30)
        };
        _context.PayrollMonths.Add(payMonth);

        _context.Payrolls.Add(new Payroll
        {
            EmployeeId = employee.Id,
            PayrollMonthId = payMonth.Id,
            GrossSalary = 75000,
            NetSalary = 60000,
            Status = PayrollStatus.Draft
        });
        await _context.SaveChangesAsync();

        _mockCalc.Setup(c => c.CalculateAsync(employee.Id, 6, 2025))
            .ReturnsAsync(new SalaryBreakdownDto { GrossEarnings = 75000, NetSalary = 60000 });

        var request = new ProcessPayrollRequest
        {
            EmployeeIds = [employee.Id],
            Month = 6,
            Year = 2025
        };

        var result = await _service.ProcessAsync(request);

        var payrolls = await _context.Payrolls.CountAsync();
        payrolls.Should().Be(1);
    }

    [Fact]
    public async Task UpdatePayroll_ShouldRecalculateNetSalary()
    {
        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            EmployeeCode = "EMP003",
            FirstName = "Update",
            LastName = "Test",
            IsActive = true
        };
        _context.Employees.Add(employee);

        var payMonth = new PayrollMonth
        {
            Id = Guid.NewGuid(),
            Month = 6,
            Year = 2025,
            StartDate = new DateTime(2025, 6, 1),
            EndDate = new DateTime(2025, 6, 30)
        };
        _context.PayrollMonths.Add(payMonth);

        var payroll = new Payroll
        {
            Id = Guid.NewGuid(),
            EmployeeId = employee.Id,
            PayrollMonthId = payMonth.Id,
            GrossSalary = 75000,
            TaxDeduction = 5000,
            OtherDeductions = 2000,
            NetSalary = 68000,
            Status = PayrollStatus.Draft
        };
        _context.Payrolls.Add(payroll);
        await _context.SaveChangesAsync();

        var updateRequest = new UpdatePayrollRequest
        {
            TaxDeduction = 8000,
            OtherDeductions = 3000
        };

        var result = await _service.UpdateAsync(payroll.Id, updateRequest);

        result.Should().NotBeNull();
        result.TaxDeduction.Should().Be(8000);
        result.OtherDeductions.Should().Be(3000);
        result.NetSalary.Should().Be(64000);
    }
}
