namespace PayrollApi.Models.DTOs;

public class LeaveDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
    public string LeaveType { get; set; } = string.Empty;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public decimal TotalDays { get; set; }
    public string? Reason { get; set; }
    public string Status { get; set; } = "Pending";
    public DateTime AppliedOn { get; set; }
}

public class LeaveListResponse
{
    public List<LeaveDto> Items { get; set; } = new();
    public int Total { get; set; }
}

public class CreateLeaveRequest
{
    public Guid EmployeeId { get; set; }
    public Guid LeaveTypeId { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public string? Reason { get; set; }
}

public class UpdateLeaveRequest
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? Reason { get; set; }
}

public class ApproveLeaveRequest
{
    public string? Comments { get; set; }
}

public class LeaveBalanceDto
{
    public Guid LeaveTypeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public decimal Taken { get; set; }
    public decimal Balance { get; set; }
    public bool IsPaid { get; set; }
}
