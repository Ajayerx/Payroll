using PayrollApi.Models.DTOs;

namespace PayrollApi.Services;

public interface IPayrollService
{
    Task<PayrollListResponse> GetAllAsync(int? month, int? year, string? status, int page, int pageSize);
    Task<PayrollDto> GetByIdAsync(Guid id);
    Task<PayrollDto> ProcessAsync(ProcessPayrollRequest request);
    Task<PayrollDto> UpdateAsync(Guid id, UpdatePayrollRequest request);
    Task<byte[]> GetSalarySlipAsync(Guid id);
    Task<string> GenerateSlipAsync(Guid id);
    Task<PayrollListResponse> GetByMonthYearAsync(int month, int year);
    Task<List<PayrollDto>> BulkProcessAsync(BulkProcessPayrollRequest request);
    Task<byte[]> ExportCsvAsync(int? month, int? year, string? status);
    Task<byte[]> ExportPdfAsync(Guid id);
}
