using PayrollApi.Models.DTOs;

namespace PayrollApi.Services;

public interface ISalaryService
{
    Task<List<SalaryComponentDto>> GetComponentsAsync();
    Task<SalaryComponentDto> CreateComponentAsync(CreateSalaryComponentRequest request);
    Task<SalaryComponentDto> UpdateComponentAsync(Guid id, UpdateSalaryComponentRequest request);
    Task<List<EmployeeSalaryStructureDto>> GetEmployeeStructureAsync(Guid employeeId);
    Task UpdateEmployeeStructureAsync(Guid employeeId, UpdateSalaryStructureRequest request);
    Task<List<DeductionDto>> GetEmployeeDeductionsAsync(Guid employeeId);
    Task<DeductionDto> AddEmployeeDeductionAsync(Guid employeeId, CreateDeductionRequest request);
    Task<DeductionDto> UpdateEmployeeDeductionAsync(Guid employeeId, Guid deductionId, CreateDeductionRequest request);
    Task DeleteEmployeeDeductionAsync(Guid employeeId, Guid deductionId);
}
