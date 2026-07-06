using Microsoft.EntityFrameworkCore;
using PayrollApi.Data;
using PayrollApi.Models.DTOs;
using PayrollApi.Services.Interfaces;

namespace PayrollApi.Services;

public class LookupCacheService : ILookupCacheService
{
    private readonly ICacheService _cache;
    private readonly PayrollDbContext _context;

    public LookupCacheService(ICacheService cache, PayrollDbContext context)
    {
        _cache = cache;
        _context = context;
    }

    public async Task<List<SalaryComponentDto>> GetSalaryComponentsAsync()
    {
        return await _cache.GetOrSetAsync("SalaryComponents", async () =>
        {
            return await _context.SalaryComponents
                .Where(sc => sc.IsActive && !sc.IsDeleted)
                .Select(sc => new SalaryComponentDto
                {
                    Id = sc.Id,
                    Name = sc.Name,
                    Type = sc.Type.ToString(),
                    IsVariable = sc.IsVariable,
                    Description = sc.Description
                })
                .ToListAsync();
        }, 10);
    }

    public async Task<List<LeaveTypeDto>> GetLeaveTypesAsync()
    {
        return await _cache.GetOrSetAsync("LeaveTypes", async () =>
        {
            return await _context.LeaveTypes
                .Where(lt => lt.IsActive && !lt.IsDeleted)
                .Select(lt => new LeaveTypeDto
                {
                    Id = lt.Id,
                    Name = lt.Name,
                    DaysPerYear = lt.DaysPerYear,
                    IsPaid = lt.IsPaid
                })
                .ToListAsync();
        }, 10);
    }

    public Task InvalidateSalaryComponentsAsync()
    {
        _cache.Remove("SalaryComponents");
        return Task.CompletedTask;
    }

    public Task InvalidateLeaveTypesAsync()
    {
        _cache.Remove("LeaveTypes");
        return Task.CompletedTask;
    }
}
