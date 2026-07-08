using Microsoft.EntityFrameworkCore;
using PayrollApi.Constants;
using PayrollApi.Data;
using PayrollApi.Models.DTOs;
using PayrollApi.Models.Entities;
using PayrollApi.Services.Interfaces;

namespace PayrollApi.Services;

public class SettingsService : ISettingsService
{
    private readonly PayrollDbContext _context;
    private readonly IAuditService _auditService;
    private readonly ILookupCacheService _lookupCache;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SettingsService(PayrollDbContext context, IAuditService auditService, ILookupCacheService lookupCache, IHttpContextAccessor httpContextAccessor)
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

    public async Task<CompanySettingDto> GetCompanyAsync()
    {
        var setting = await _context.CompanySettings.FirstOrDefaultAsync();
        if (setting == null)
        {
            setting = new CompanySetting { CompanyName = "My Company" };
            _context.CompanySettings.Add(setting);
            await _context.SaveChangesAsync();
        }

        return new CompanySettingDto
        {
            Id = setting.Id,
            CompanyName = setting.CompanyName,
            Address = setting.Address,
            Email = setting.Email,
            Phone = setting.Phone,
            Gstin = setting.Gstin,
            Pan = setting.Pan,
            LogoUrl = setting.LogoUrl
        };
    }

    public async Task<CompanySettingDto> UpdateCompanyAsync(UpdateCompanySettingRequest request)
    {
        var setting = await _context.CompanySettings.FirstOrDefaultAsync();
        if (setting == null)
        {
            setting = new CompanySetting();
            _context.CompanySettings.Add(setting);
        }

        if (request.CompanyName != null) setting.CompanyName = request.CompanyName;
        if (request.Address != null) setting.Address = request.Address;
        if (request.Email != null) setting.Email = request.Email;
        if (request.Phone != null) setting.Phone = request.Phone;
        if (request.Gstin != null) setting.Gstin = request.Gstin;
        if (request.Pan != null) setting.Pan = request.Pan;

        setting.UpdatedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        await _auditService.LogAsync(EntityNames.CompanySetting, setting.Id.ToString(), "Update", null, request, CurrentUserId, CurrentUserIp);

        return await GetCompanyAsync();
    }

    public async Task<List<TaxSlabDto>> GetTaxSlabsAsync()
    {
        return await _context.TaxSlabs
            .Where(ts => ts.IsActive)
            .Select(ts => new TaxSlabDto
            {
                Id = ts.Id,
                Name = ts.Name,
                FromAmount = ts.FromAmount,
                ToAmount = ts.ToAmount,
                Rate = ts.Rate,
                EffectiveDate = ts.EffectiveDate
            })
            .ToListAsync();
    }

    public async Task<TaxSlabDto> CreateTaxSlabAsync(CreateTaxSlabRequest request)
    {
        var slab = new TaxSlab
        {
            Name = request.Name,
            FromAmount = request.FromAmount,
            ToAmount = request.ToAmount,
            Rate = request.Rate,
            IsActive = true
        };

        _context.TaxSlabs.Add(slab);
        await _context.SaveChangesAsync();

        await _auditService.LogAsync("TaxSlab", slab.Id.ToString(), "Create", null, new { slab.Name, slab.FromAmount, slab.ToAmount, slab.Rate }, CurrentUserId, CurrentUserIp);

        return new TaxSlabDto
        {
            Id = slab.Id,
            Name = slab.Name,
            FromAmount = slab.FromAmount,
            ToAmount = slab.ToAmount,
            Rate = slab.Rate,
            EffectiveDate = slab.EffectiveDate
        };
    }

    public async Task<List<LeaveTypeDto>> GetLeaveTypesAsync()
    {
        return await _context.LeaveTypes
            .Where(lt => lt.IsActive)
            .Select(lt => new LeaveTypeDto
            {
                Id = lt.Id,
                Name = lt.Name,
                DaysPerYear = lt.DaysPerYear,
                IsPaid = lt.IsPaid
            })
            .ToListAsync();
    }

    public async Task<LeaveTypeDto> CreateLeaveTypeAsync(CreateLeaveTypeRequest request)
    {
        var leaveType = new LeaveType
        {
            Name = request.Name,
            DaysPerYear = request.DaysPerYear,
            IsPaid = request.IsPaid,
            IsActive = true
        };

        _context.LeaveTypes.Add(leaveType);
        await _context.SaveChangesAsync();

        await _lookupCache.InvalidateLeaveTypesAsync();
        await _auditService.LogAsync(EntityNames.LeaveRequest, leaveType.Id.ToString(), "Create", null, new { leaveType.Name, leaveType.DaysPerYear, leaveType.IsPaid }, CurrentUserId, CurrentUserIp);

        return new LeaveTypeDto
        {
            Id = leaveType.Id,
            Name = leaveType.Name,
            DaysPerYear = leaveType.DaysPerYear,
            IsPaid = leaveType.IsPaid
        };
    }
}
