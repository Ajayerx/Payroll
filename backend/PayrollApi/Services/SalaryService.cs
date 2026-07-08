using Microsoft.EntityFrameworkCore;
using PayrollApi.Constants;
using PayrollApi.Data;
using PayrollApi.Models.DTOs;
using PayrollApi.Models.Entities;
using PayrollApi.Models.Enums;
using PayrollApi.Services.Interfaces;

namespace PayrollApi.Services;

public class SalaryService : ISalaryService
{
    private readonly PayrollDbContext _context;
    private readonly IAuditService _auditService;
    private readonly ILookupCacheService _lookupCache;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SalaryService(PayrollDbContext context, IAuditService auditService, ILookupCacheService lookupCache, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _auditService = auditService;
        _lookupCache = lookupCache;
        _httpContextAccessor = httpContextAccessor;
    }

    private Guid CurrentUserId =>
        Guid.TryParse(_httpContextAccessor.HttpContext?.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var id) ? id : Guid.Empty;

    private string? CurrentUserIp =>
        _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

    public async Task<List<SalaryComponentDto>> GetComponentsAsync()
    {
        return await _context.SalaryComponents
            .Where(sc => sc.IsActive)
            .Select(sc => new SalaryComponentDto
            {
                Id = sc.Id,
                Name = sc.Name,
                Type = sc.Type.ToString(),
                IsVariable = sc.IsVariable,
                Description = sc.Description
            })
            .ToListAsync();
    }

    public async Task<SalaryComponentDto> CreateComponentAsync(CreateSalaryComponentRequest request)
    {
        var component = new SalaryComponent
        {
            Name = request.Name,
            Type = Enum.Parse<SalaryComponentType>(request.Type, true),
            IsVariable = request.IsVariable,
            Description = request.Description
        };

        _context.SalaryComponents.Add(component);
        await _context.SaveChangesAsync();

        await _lookupCache.InvalidateSalaryComponentsAsync();
        await _auditService.LogAsync(EntityNames.SalaryComponent, component.Id.ToString(), "Create", null, new { component.Name, component.Type }, CurrentUserId, CurrentUserIp);

        return new SalaryComponentDto
        {
            Id = component.Id,
            Name = component.Name,
            Type = component.Type.ToString(),
            IsVariable = component.IsVariable,
            Description = component.Description
        };
    }

    public async Task<SalaryComponentDto> UpdateComponentAsync(Guid id, UpdateSalaryComponentRequest request)
    {
        var component = await _context.SalaryComponents.FindAsync(id)
            ?? throw new KeyNotFoundException($"Salary component with ID {id} not found");

        if (request.Name != null) component.Name = request.Name;
        if (request.Type != null) component.Type = Enum.Parse<SalaryComponentType>(request.Type, true);
        if (request.IsVariable.HasValue) component.IsVariable = request.IsVariable.Value;
        if (request.Description != null) component.Description = request.Description;

        await _context.SaveChangesAsync();

        await _lookupCache.InvalidateSalaryComponentsAsync();
        await _auditService.LogAsync(EntityNames.SalaryComponent, component.Id.ToString(), "Update", null, new { component.Name, component.Type }, CurrentUserId, CurrentUserIp);

        return new SalaryComponentDto
        {
            Id = component.Id,
            Name = component.Name,
            Type = component.Type.ToString(),
            IsVariable = component.IsVariable,
            Description = component.Description
        };
    }

    public async Task<List<EmployeeSalaryStructureDto>> GetEmployeeStructureAsync(Guid employeeId)
    {
        return await _context.EmployeeSalaryStructures
            .Where(ess => ess.EmployeeId == employeeId)
            .Include(ess => ess.SalaryComponent)
            .Select(ess => new EmployeeSalaryStructureDto
            {
                Id = ess.Id,
                EmployeeId = ess.EmployeeId,
                SalaryComponentId = ess.SalaryComponentId,
                ComponentName = ess.SalaryComponent.Name,
                ComponentType = ess.SalaryComponent.Type.ToString(),
                Amount = ess.Amount,
                EffectiveDate = ess.EffectiveDate
            })
            .ToListAsync();
    }

    public async Task UpdateEmployeeStructureAsync(Guid employeeId, UpdateSalaryStructureRequest request)
    {
        var existing = await _context.EmployeeSalaryStructures
            .Where(ess => ess.EmployeeId == employeeId)
            .ToListAsync();

        _context.EmployeeSalaryStructures.RemoveRange(existing);

        foreach (var comp in request.Components)
        {
            _context.EmployeeSalaryStructures.Add(new EmployeeSalaryStructure
            {
                EmployeeId = employeeId,
                SalaryComponentId = comp.SalaryComponentId,
                Amount = comp.Amount,
                EffectiveDate = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();
    }

    public async Task<List<DeductionDto>> GetEmployeeDeductionsAsync(Guid employeeId)
    {
        return await _context.Deductions
            .Where(d => d.EmployeeId == employeeId)
            .Select(d => new DeductionDto
            {
                Id = d.Id,
                EmployeeId = d.EmployeeId,
                Type = d.Type.ToString(),
                Amount = d.Amount,
                RemainingAmount = d.RemainingAmount,
                StartDate = d.StartDate,
                EndDate = d.EndDate
            })
            .ToListAsync();
    }

    public async Task<DeductionDto> AddEmployeeDeductionAsync(Guid employeeId, CreateDeductionRequest request)
    {
        var deduction = new Deduction
        {
            EmployeeId = employeeId,
            Type = Enum.Parse<DeductionType>(request.Type, true),
            Amount = request.Amount,
            RemainingAmount = request.Amount,
            StartDate = request.StartDate,
            EndDate = request.EndDate
        };

        _context.Deductions.Add(deduction);
        await _context.SaveChangesAsync();

        await _auditService.LogAsync(EntityNames.Deduction, deduction.Id.ToString(), "Create", null, new { deduction.EmployeeId, deduction.Type, deduction.Amount }, CurrentUserId, CurrentUserIp);

        return new DeductionDto
        {
            Id = deduction.Id,
            EmployeeId = deduction.EmployeeId,
            Type = deduction.Type.ToString(),
            Amount = deduction.Amount,
            RemainingAmount = deduction.RemainingAmount,
            StartDate = deduction.StartDate,
            EndDate = deduction.EndDate
        };
    }

    public async Task<DeductionDto> UpdateEmployeeDeductionAsync(Guid employeeId, Guid deductionId, CreateDeductionRequest request)
    {
        var deduction = await _context.Deductions.FirstOrDefaultAsync(d => d.Id == deductionId && d.EmployeeId == employeeId)
            ?? throw new KeyNotFoundException($"Deduction {deductionId} not found");

        deduction.Type = Enum.Parse<DeductionType>(request.Type, true);
        deduction.Amount = request.Amount;
        deduction.RemainingAmount = request.Amount;
        deduction.StartDate = request.StartDate;
        deduction.EndDate = request.EndDate;
        await _context.SaveChangesAsync();

        await _auditService.LogAsync(EntityNames.Deduction, deduction.Id.ToString(), "Update", null, new { deduction.EmployeeId, deduction.Type, deduction.Amount }, CurrentUserId, CurrentUserIp);

        return new DeductionDto
        {
            Id = deduction.Id,
            EmployeeId = deduction.EmployeeId,
            Type = deduction.Type.ToString(),
            Amount = deduction.Amount,
            RemainingAmount = deduction.RemainingAmount,
            StartDate = deduction.StartDate,
            EndDate = deduction.EndDate
        };
    }

    public async Task DeleteEmployeeDeductionAsync(Guid employeeId, Guid deductionId)
    {
        var deduction = await _context.Deductions.FirstOrDefaultAsync(d => d.Id == deductionId && d.EmployeeId == employeeId)
            ?? throw new KeyNotFoundException($"Deduction {deductionId} not found");

        _context.Deductions.Remove(deduction);
        await _context.SaveChangesAsync();

        await _auditService.LogAsync(EntityNames.Deduction, deduction.Id.ToString(), "Delete", null, new { deduction.EmployeeId, deduction.Type }, CurrentUserId, CurrentUserIp);
    }
}
