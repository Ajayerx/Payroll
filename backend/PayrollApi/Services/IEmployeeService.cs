using PayrollApi.Models.DTOs;

namespace PayrollApi.Services;

public interface IEmployeeService
{
    Task<EmployeeListResponse> GetAllAsync(int page, int pageSize, string? search, string? department, string? status);
    Task<EmployeeDto> GetByIdAsync(Guid id);
    Task<EmployeeDto> CreateAsync(CreateEmployeeRequest request);
    Task<EmployeeDto> UpdateAsync(Guid id, UpdateEmployeeRequest request);
    Task DeleteAsync(Guid id);
    Task<EmployeeListResponse> SearchAsync(string query);
}
