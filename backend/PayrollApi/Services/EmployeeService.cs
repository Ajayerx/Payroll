using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;
using PayrollApi.Constants;
using PayrollApi.Data;
using PayrollApi.Models.DTOs;
using PayrollApi.Models.Entities;
using PayrollApi.Models.Enums;
using PayrollApi.Services.Interfaces;
using PayrollApi.Utils;

namespace PayrollApi.Services;

public class EmployeeService : IEmployeeService
{
    private readonly PayrollDbContext _context;
    private readonly IAuditService _auditService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public EmployeeService(PayrollDbContext context, IAuditService auditService, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _auditService = auditService;
        _httpContextAccessor = httpContextAccessor;
    }

    private Guid CurrentUserId =>
        Guid.TryParse(_httpContextAccessor.HttpContext?.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var id) ? id : Guid.Empty;

    private string? CurrentUserIp =>
        _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

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

    public async Task<EmployeeDto> GetByUserIdAsync(Guid userId)
    {
        var employee = await _context.Employees.FirstOrDefaultAsync(e => e.UserId == userId)
            ?? throw new KeyNotFoundException("Employee profile not found");
        return MapDto(employee);
    }

    public async Task<EmployeeDto> CreateAsync(CreateEmployeeRequest request)
    {
        var tempPassword = PasswordPolicy.GenerateTempPassword();
        var user = new User
        {
            Email = request.Email ?? $"{request.FirstName.ToLower()}.{request.LastName.ToLower()}@company.com",
            Password = BCrypt.Net.BCrypt.HashPassword(tempPassword),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Role = Models.Enums.UserRole.Employee,
            IsActive = true
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        await _auditService.LogAsync(EntityNames.User, user.Id.ToString(), "Create", null, new { user.Email, user.FirstName, user.LastName, user.Role }, CurrentUserId, CurrentUserIp);

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

        await _auditService.LogAsync(EntityNames.Employee, employee.Id.ToString(), "Create", null, MapDto(employee), CurrentUserId, CurrentUserIp);

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
        employee.UpdatedBy = CurrentUserId;
        await _context.SaveChangesAsync();

        await _auditService.LogAsync(EntityNames.Employee, employee.Id.ToString(), "Update", null, MapDto(employee), CurrentUserId, CurrentUserIp);

        return MapDto(employee);
    }

    public async Task DeleteAsync(Guid id)
    {
        var employee = await _context.Employees.FindAsync(id)
            ?? throw new KeyNotFoundException($"Employee with ID {id} not found");

        employee.IsActive = false;
        employee.UpdatedDate = DateTime.UtcNow;
        employee.UpdatedBy = CurrentUserId;
        await _context.SaveChangesAsync();

        await _auditService.LogAsync(EntityNames.Employee, employee.Id.ToString(), "Delete", null, new { employee.IsActive }, CurrentUserId, CurrentUserIp);
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

    public async Task<BulkImportResult> BulkImportAsync(IFormFile file)
    {
        var result = new BulkImportResult();
        using var reader = new StreamReader(file.OpenReadStream());
        var header = await reader.ReadLineAsync();
        if (header == null) return result;

        string? line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            try
            {
                var parts = line.Split(',');
                if (parts.Length < 2) { result.Failed++; result.Errors.Add($"Invalid line: {line}"); continue; }

                var firstName = parts[0].Trim();
                var lastName = parts[1].Trim();
                var email = parts.Length > 2 ? parts[2].Trim() : $"{firstName.ToLower()}.{lastName.ToLower()}@company.com";
                var department = parts.Length > 3 ? parts[3].Trim() : null;
                var designation = parts.Length > 4 ? parts[4].Trim() : null;

                var tempPassword = PasswordPolicy.GenerateTempPassword();
                var user = new User
                {
                    Email = email,
                    Password = BCrypt.Net.BCrypt.HashPassword(tempPassword),
                    FirstName = firstName,
                    LastName = lastName,
                    Role = UserRole.Employee,
                    IsActive = true
                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                var employee = new Employee
                {
                    UserId = user.Id,
                    EmployeeCode = $"EMP{DateTime.UtcNow:yyyyMMdd}{_context.Employees.Count() + 1}",
                    FirstName = firstName,
                    LastName = lastName,
                    Email = email,
                    Department = department,
                    Designation = designation,
                    DateOfJoining = DateTime.UtcNow,
                    IsActive = true
                };
                _context.Employees.Add(employee);
                await _context.SaveChangesAsync();
                result.Imported++;
            }
            catch (Exception ex)
            {
                result.Failed++;
                result.Errors.Add($"Error importing line '{line}': {ex.Message}");
            }
        }
        return result;
    }

    public async Task<List<EmployeeDocumentDto>> GetDocumentsAsync(Guid employeeId)
    {
        return await _context.EmployeeDocuments
            .Where(d => d.EmployeeId == employeeId)
            .OrderByDescending(d => d.UploadedDate)
            .Select(d => new EmployeeDocumentDto
            {
                Id = d.Id,
                FileName = d.FileName,
                ContentType = d.ContentType,
                FileSize = d.FileSize,
                Category = d.Category,
                UploadedDate = d.UploadedDate
            })
            .ToListAsync();
    }

    public async Task<EmployeeDocumentDto> UploadDocumentAsync(Guid employeeId, IFormFile file, string? category)
    {
        var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", employeeId.ToString());
        Directory.CreateDirectory(uploadsDir);

        var fileName = $"{Guid.NewGuid()}_{file.FileName}";
        var filePath = Path.Combine(uploadsDir, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var doc = new EmployeeDocument
        {
            EmployeeId = employeeId,
            FileName = file.FileName,
            ContentType = file.ContentType,
            FileSize = file.Length,
            FilePath = filePath,
            Category = category
        };

        _context.EmployeeDocuments.Add(doc);
        await _context.SaveChangesAsync();

        await _auditService.LogAsync("EmployeeDocument", doc.Id.ToString(), "Upload", null,
            new { doc.EmployeeId, doc.FileName, doc.Category }, CurrentUserId, CurrentUserIp);

        return new EmployeeDocumentDto
        {
            Id = doc.Id,
            FileName = doc.FileName,
            ContentType = doc.ContentType,
            FileSize = doc.FileSize,
            Category = doc.Category,
            UploadedDate = doc.UploadedDate
        };
    }

    public async Task DeleteDocumentAsync(Guid employeeId, Guid documentId)
    {
        var doc = await _context.EmployeeDocuments
            .FirstOrDefaultAsync(d => d.Id == documentId && d.EmployeeId == employeeId)
            ?? throw new KeyNotFoundException("Document not found");

        if (File.Exists(doc.FilePath))
            File.Delete(doc.FilePath);

        _context.EmployeeDocuments.Remove(doc);
        await _context.SaveChangesAsync();

        await _auditService.LogAsync("EmployeeDocument", doc.Id.ToString(), "Delete", null,
            new { doc.EmployeeId, doc.FileName }, CurrentUserId, CurrentUserIp);
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
