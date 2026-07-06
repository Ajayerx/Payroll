using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PayrollApi.Data;
using PayrollApi.Models.DTOs;
using PayrollApi.Models.Entities;
using PayrollApi.Services;

namespace PayrollApi.Tests;

public class EmployeeServiceTests : IDisposable
{
    private readonly PayrollDbContext _context;
    private readonly EmployeeService _service;

    public EmployeeServiceTests()
    {
        var options = new DbContextOptionsBuilder<PayrollDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new PayrollDbContext(options);
        _service = new EmployeeService(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task GetAllAsync_WithNoEmployees_ShouldReturnEmptyList()
    {
        var result = await _service.GetAllAsync(1, 20, null, null, null);
        result.Items.Should().BeEmpty();
        result.Total.Should().Be(0);
    }

    [Fact]
    public async Task CreateEmployee_ShouldSetEmployeeCode()
    {
        var request = new CreateEmployeeRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@test.com",
            Department = "Engineering"
        };

        var result = await _service.CreateAsync(request);

        result.Should().NotBeNull();
        result.EmployeeCode.Should().NotBeNullOrEmpty();
        result.EmployeeCode.Should().StartWith("ENG");
        result.FirstName.Should().Be("John");
        result.LastName.Should().Be("Doe");
    }

    [Fact]
    public async Task Create_And_GetById_ShouldReturnSameEmployee()
    {
        var createRequest = new CreateEmployeeRequest
        {
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane.smith@test.com",
            Department = "HR",
            Designation = "Manager"
        };

        var created = await _service.CreateAsync(createRequest);
        var fetched = await _service.GetByIdAsync(created.Id);

        fetched.Should().NotBeNull();
        fetched.FirstName.Should().Be("Jane");
        fetched.LastName.Should().Be("Smith");
        fetched.Department.Should().Be("HR");
        fetched.Designation.Should().Be("Manager");
    }

    [Fact]
    public async Task GetByIdAsync_WithNonexistentId_ShouldThrowKeyNotFoundException()
    {
        var act = () => _service.GetByIdAsync(Guid.NewGuid());
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task UpdateEmployee_ShouldModifyFields()
    {
        var createRequest = new CreateEmployeeRequest
        {
            FirstName = "Original",
            LastName = "Name",
            Email = "original@test.com"
        };

        var created = await _service.CreateAsync(createRequest);

        var updateRequest = new UpdateEmployeeRequest
        {
            FirstName = "Updated",
            LastName = "Name",
            Department = "Finance"
        };

        var updated = await _service.UpdateAsync(created.Id, updateRequest);

        updated.FirstName.Should().Be("Updated");
        updated.Department.Should().Be("Finance");
    }

    [Fact]
    public async Task CreateMultipleEmployees_ShouldGenerateUniqueCodes()
    {
        var codes = new HashSet<string>();

        for (int i = 0; i < 5; i++)
        {
            var request = new CreateEmployeeRequest
            {
                FirstName = $"User{i}",
                LastName = "Test",
                Email = $"user{i}@test.com",
                Department = "IT"
            };

            var result = await _service.CreateAsync(request);
            codes.Add(result.EmployeeCode).Should().BeTrue($"Employee code {result.EmployeeCode} should be unique");
        }
    }

    [Fact]
    public async Task GetAllAsync_WithPagination_ShouldReturnCorrectPage()
    {
        for (int i = 0; i < 10; i++)
        {
            await _service.CreateAsync(new CreateEmployeeRequest
            {
                FirstName = $"User{i}",
                LastName = "Test",
                Email = $"user{i}@test.com"
            });
        }

        var page1 = await _service.GetAllAsync(1, 5, null, null, null);
        page1.Items.Should().HaveCount(5);
        page1.Total.Should().Be(10);

        var page2 = await _service.GetAllAsync(2, 5, null, null, null);
        page2.Items.Should().HaveCount(5);
    }

    [Fact]
    public async Task GetAllAsync_WithSearchQuery_ShouldFilterResults()
    {
        await _service.CreateAsync(new CreateEmployeeRequest { FirstName = "Alpha", LastName = "User", Email = "alpha@test.com", Department = "Engineering" });
        await _service.CreateAsync(new CreateEmployeeRequest { FirstName = "Beta", LastName = "User", Email = "beta@test.com", Department = "Sales" });
        await _service.CreateAsync(new CreateEmployeeRequest { FirstName = "Gamma", LastName = "User", Email = "gamma@test.com", Department = "Engineering" });

        var result = await _service.GetAllAsync(1, 20, null, department: "Engineering", null);

        result.Items.Should().HaveCount(2);
        result.Total.Should().Be(2);
    }
}
