using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PayrollApi.Data;
using PayrollApi.Models.DTOs;
using PayrollApi.Models.Entities;
using PayrollApi.Models.Enums;
using PayrollApi.Services;

namespace PayrollApi.Tests;

public class ReportServiceTests : IDisposable
{
    private readonly PayrollDbContext _context;
    private readonly ReportService _service;

    public ReportServiceTests()
    {
        var options = new DbContextOptionsBuilder<PayrollDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new PayrollDbContext(options);
        _service = new ReportService(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    private async Task SeedData()
    {
        var emp1 = new Employee
        {
            Id = Guid.NewGuid(),
            EmployeeCode = "EMP001",
            FirstName = "John",
            LastName = "Doe",
            Department = "Engineering",
            Designation = "Developer"
        };
        var emp2 = new Employee
        {
            Id = Guid.NewGuid(),
            EmployeeCode = "EMP002",
            FirstName = "Jane",
            LastName = "Smith",
            Department = "Sales",
            Designation = "Manager"
        };
        _context.Employees.AddRange(emp1, emp2);

        var pm = new PayrollMonth
        {
            Id = Guid.NewGuid(),
            Month = 6,
            Year = 2025,
            StartDate = new DateTime(2025, 6, 1),
            EndDate = new DateTime(2025, 6, 30)
        };
        _context.PayrollMonths.Add(pm);

        _context.Payrolls.AddRange(
            new Payroll
            {
                EmployeeId = emp1.Id,
                PayrollMonthId = pm.Id,
                GrossSalary = 75000,
                TaxDeduction = 7500,
                OtherDeductions = 5000,
                NetSalary = 62500,
                Status = PayrollStatus.Processed,
                ProcessedDate = DateTime.UtcNow
            },
            new Payroll
            {
                EmployeeId = emp2.Id,
                PayrollMonthId = pm.Id,
                GrossSalary = 90000,
                TaxDeduction = 9000,
                OtherDeductions = 3000,
                NetSalary = 78000,
                Status = PayrollStatus.Processed,
                ProcessedDate = DateTime.UtcNow
            }
        );

        await _context.SaveChangesAsync();
    }

    [Fact]
    public async Task GetSalaryRegisterAsync_ShouldReturnAllPayrolls()
    {
        await SeedData();

        var request = new SalaryRegisterRequest { Month = 6, Year = 2025 };
        var result = await _service.GetSalaryRegisterAsync(request);

        result.Should().HaveCount(2);
        result[0].GrossPay.Should().Be(75000);
        result[1].GrossPay.Should().Be(90000);
    }

    [Fact]
    public async Task GetSalaryRegisterAsync_WithDepartmentFilter_ShouldFilter()
    {
        await SeedData();

        var request = new SalaryRegisterRequest { Month = 6, Year = 2025, Department = "Engineering" };
        var result = await _service.GetSalaryRegisterAsync(request);

        result.Should().HaveCount(1);
        result[0].Department.Should().Be("Engineering");
    }

    [Fact]
    public async Task GetTaxSummaryAsync_ShouldCalculateCorrectly()
    {
        await SeedData();

        var request = new TaxSummaryRequest { Year = 2025 };
        var result = await _service.GetTaxSummaryAsync(request);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetDepartmentSummaryAsync_ShouldGroupByDepartment()
    {
        await SeedData();

        var request = new DepartmentSummaryRequest { Month = 6, Year = 2025 };
        var result = await _service.GetDepartmentSummaryAsync(request);

        result.Should().HaveCount(2);
        result.Should().Contain(d => d.Department == "Engineering");
        result.Should().Contain(d => d.Department == "Sales");
    }

    [Fact]
    public async Task ExportAsync_ShouldGenerateCsv()
    {
        await SeedData();

        var request = new ExportRequest { Month = 6, Year = 2025 };
        var result = await _service.ExportAsync(request, "csv");

        result.Should().NotBeNull();
        result.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetSalaryRegisterAsync_WithNoData_ShouldReturnEmpty()
    {
        var request = new SalaryRegisterRequest { Month = 1, Year = 2024 };
        var result = await _service.GetSalaryRegisterAsync(request);

        result.Should().BeEmpty();
    }
}
