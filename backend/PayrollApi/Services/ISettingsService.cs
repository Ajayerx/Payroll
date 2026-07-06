using PayrollApi.Models.DTOs;

namespace PayrollApi.Services;

public interface ISettingsService
{
    Task<CompanySettingDto> GetCompanyAsync();
    Task<CompanySettingDto> UpdateCompanyAsync(UpdateCompanySettingRequest request);
    Task<List<TaxSlabDto>> GetTaxSlabsAsync();
    Task<TaxSlabDto> CreateTaxSlabAsync(CreateTaxSlabRequest request);
    Task<List<LeaveTypeDto>> GetLeaveTypesAsync();
    Task<LeaveTypeDto> CreateLeaveTypeAsync(CreateLeaveTypeRequest request);
}
