using PayrollApi.Models.DTOs;

namespace PayrollApi.Services;

public interface IEmployeeService
{
    Task<EmployeeListResponse> GetAllAsync(int page, int pageSize, string? search, string? department, string? status);
    Task<EmployeeDto> GetByIdAsync(Guid id);
    Task<EmployeeDto> GetByUserIdAsync(Guid userId);
    Task<EmployeeDto> CreateAsync(CreateEmployeeRequest request);
    Task<EmployeeDto> UpdateAsync(Guid id, UpdateEmployeeRequest request);
    Task DeleteAsync(Guid id);
    Task<EmployeeListResponse> SearchAsync(string query);
    Task<BulkImportResult> BulkImportAsync(IFormFile file);
    Task<List<EmployeeDocumentDto>> GetDocumentsAsync(Guid employeeId);
    Task<EmployeeDocumentDto> UploadDocumentAsync(Guid employeeId, IFormFile file, string? category);
    Task DeleteDocumentAsync(Guid employeeId, Guid documentId);
}
