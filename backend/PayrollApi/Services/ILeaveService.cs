using PayrollApi.Models.DTOs;

namespace PayrollApi.Services;

public interface ILeaveService
{
    Task<LeaveListResponse> GetAllAsync(string? status, string? type, int page, int pageSize);
    Task<LeaveDto> GetByIdAsync(Guid id);
    Task<LeaveDto> CreateAsync(CreateLeaveRequest request, Guid createdBy);
    Task<LeaveDto> UpdateAsync(Guid id, UpdateLeaveRequest request);
    Task<LeaveDto> ApproveAsync(Guid id, Guid approvedBy, string? comments);
    Task<LeaveDto> RejectAsync(Guid id, Guid rejectedBy, string? comments);
    Task CancelAsync(Guid id);
    Task<List<LeaveDto>> GetByEmployeeAsync(Guid employeeId, string? status);
    Task<List<LeaveTypeDto>> GetLeaveTypesAsync();
    Task<List<LeaveBalanceDto>> GetLeaveBalanceAsync(Guid employeeId);
}
