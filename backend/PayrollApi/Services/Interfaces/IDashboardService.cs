using PayrollApi.Models.DTOs;

namespace PayrollApi.Services.Interfaces;

public interface IDashboardService
{
    Task<DashboardDto> GetDashboardAsync(int? month, int? year);
}
