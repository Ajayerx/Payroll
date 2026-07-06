using Microsoft.EntityFrameworkCore;
using PayrollApi.Data;
using PayrollApi.Models.DTOs;
using PayrollApi.Models.Entities;

namespace PayrollApi.Services;

public class EmployeeService : IEmployeeService
{
    private readonly PayrollDbContext _context;

    public EmployeeService(PayrollDbContext context)
    {
        _context = context;
    }

    public async Task<EmployeeListResponse> GetAllAsync(int page, int pageSize, string? search, string? department, string? status)
    {
        var query = _context.Employees.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(e =>
                e.FirstName.ToLower().Contains(term) ||
                e.LastName.ToLower().Contains(term) ||
                e.EmployeeCode.ToLower().Contains(term) ||
                (e.Email != null && e.Email.ToLower().Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(department))
            query = query.Where(e => e.Department == department);

        if (!string.IsNullOrWhiteSpace(status))
        {
            var isActive = status.Equals("Active", StringComparison.OrdinalIgnoreCase);
            query = query.Where(e => e.IsActive == isActive);
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(e => e.CreatedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => MapDto(e))
            .ToListAsync();

        return new EmployeeListResponse
        {
            Items = items,
            Total = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<EmployeeDto> GetByIdAsync(Guid id)
    {
        var employee = await _context.Employees.FindAsync(id)
            ?? throw new KeyNotFoundException($"Employee with ID {id} not found");
        return MapDto(employee);
    }

    public async Task<EmployeeDto> CreateAsync(CreateEmployeeRequest request)
    {
        var user = new User
        {
            Email = request.Email ?? $"{request.FirstName.ToLower()}.{request.LastName.ToLower()}@company.com",
            Password = BCrypt.Net.BCrypt.HashPassword("Welcome@123"),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Role = Models.Enums.UserRole.Employee,
            IsActive = true
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var code = await GenerateEmployeeCode(request.Department ?? "EMP");

        var employee = new Employee
        {
            UserId = user.Id,
            EmployeeCode = code,
            FirstName = request.FirstName,
            LastName = request.LastName,
            DOB = request.DOB,
            Gender = request.Gender,
            Phone = request.Phone,
            Email = request.Email,
            Address = request.Address,
            City = request.City,
            State = request.State,
            Pincode = request.Pincode,
            DateOfJoining = request.DateOfJoining,
            Department = request.Department,
            Designation = request.Designation,
            BankName = request.BankName,
            BankAccount = request.BankAccount,
            IfscCode = request.IfscCode,
            IsActive = true
        };

        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();

        return MapDto(employee);
    }

    public async Task<EmployeeDto> UpdateAsync(Guid id, UpdateEmployeeRequest request)
    {
        var employee = await _context.Employees.FindAsync(id)
            ?? throw new KeyNotFoundException($"Employee with ID {id} not found");

        if (request.FirstName != null) employee.FirstName = request.FirstName;
        if (request.LastName != null) employee.LastName = request.LastName;
        if (request.DOB.HasValue) employee.DOB = request.DOB;
        if (request.Gender != null) employee.Gender = request.Gender;
        if (request.Phone != null) employee.Phone = request.Phone;
        if (request.Email != null) employee.Email = request.Email;
        if (request.Address != null) employee.Address = request.Address;
        if (request.City != null) employee.City = request.City;
        if (request.State != null) employee.State = request.State;
        if (request.Pincode != null) employee.Pincode = request.Pincode;
        if (request.DateOfJoining.HasValue) employee.DateOfJoining = request.DateOfJoining;
        if (request.Department != null) employee.Department = request.Department;
        if (request.Designation != null) employee.Designation = request.Designation;
        if (request.BankName != null) employee.BankName = request.BankName;
        if (request.BankAccount != null) employee.BankAccount = request.BankAccount;
        if (request.IfscCode != null) employee.IfscCode = request.IfscCode;
        if (request.IsActive.HasValue) employee.IsActive = request.IsActive.Value;

        employee.UpdatedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return MapDto(employee);
    }

    public async Task DeleteAsync(Guid id)
    {
        var employee = await _context.Employees.FindAsync(id)
            ?? throw new KeyNotFoundException($"Employee with ID {id} not found");

        employee.IsActive = false;
        employee.UpdatedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task<EmployeeListResponse> SearchAsync(string query)
    {
        var term = query.ToLower();
        var items = await _context.Employees
            .Where(e => e.FirstName.ToLower().Contains(term) ||
                        e.LastName.ToLower().Contains(term) ||
                        e.EmployeeCode.ToLower().Contains(term))
            .Take(20)
            .Select(e => MapDto(e))
            .ToListAsync();

        return new EmployeeListResponse { Items = items, Total = items.Count, Page = 1, PageSize = 20 };
    }

    private async Task<string> GenerateEmployeeCode(string department)
    {
        var prefix = department.Length >= 3 ? department[..3].ToUpper() : department.ToUpper();
        var count = await _context.Employees.CountAsync(e => e.Department == department);
        return $"{prefix}{(count + 1).ToString().PadLeft(4, '0')}";
    }

    private static EmployeeDto MapDto(Employee e) => new()
    {
        Id = e.Id,
        UserId = e.UserId,
        EmployeeCode = e.EmployeeCode,
        FirstName = e.FirstName,
        LastName = e.LastName,
        DOB = e.DOB,
        Gender = e.Gender,
        Phone = e.Phone,
        Email = e.Email,
        Address = e.Address,
        City = e.City,
        State = e.State,
        Pincode = e.Pincode,
        DateOfJoining = e.DateOfJoining,
        Department = e.Department,
        Designation = e.Designation,
        BankName = e.BankName,
        BankAccount = e.BankAccount,
        IfscCode = e.IfscCode,
        IsActive = e.IsActive,
    };
}
