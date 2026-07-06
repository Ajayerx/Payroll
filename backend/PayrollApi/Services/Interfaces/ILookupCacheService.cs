using PayrollApi.Models.DTOs;

namespace PayrollApi.Services.Interfaces;

public interface ILookupCacheService
{
    Task<List<SalaryComponentDto>> GetSalaryComponentsAsync();
    Task<List<LeaveTypeDto>> GetLeaveTypesAsync();
    Task InvalidateSalaryComponentsAsync();
    Task InvalidateLeaveTypesAsync();
}
