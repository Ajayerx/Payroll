using PayrollApi.Models.DTOs;

namespace PayrollApi.Services;

public interface IReportService
{
    Task<List<SalaryRegisterDto>> GetSalaryRegisterAsync(SalaryRegisterRequest request);
    Task<object> GetTaxSummaryAsync(TaxSummaryRequest request);
    Task<List<SalaryRegisterDto>> GetEmployeeEarningsAsync(EmployeeEarningsRequest request);
    Task<List<DepartmentSummaryDto>> GetDepartmentSummaryAsync(DepartmentSummaryRequest request);
    Task<byte[]> ExportAsync(ExportRequest request, string format);
}
