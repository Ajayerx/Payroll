using Microsoft.EntityFrameworkCore;
using PayrollApi.Constants;
using PayrollApi.Data;
using PayrollApi.Models.DTOs;
using PayrollApi.Models.Entities;
using PayrollApi.Services.Interfaces;

namespace PayrollApi.Services;

public class LeaveService : ILeaveService
{
    private readonly PayrollDbContext _context;
    private readonly IAuditService _auditService;
    private readonly INotificationService _notificationService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LeaveService(PayrollDbContext context, IAuditService auditService,
        INotificationService notificationService, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _auditService = auditService;
        _notificationService = notificationService;
        _httpContextAccessor = httpContextAccessor;
    }

    private Guid CurrentUserId =>
        Guid.TryParse(_httpContextAccessor.HttpContext?.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var id) ? id : Guid.Empty;

    private string? CurrentUserIp =>
        _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

    public async Task<LeaveListResponse> GetAllAsync(string? status, string? type, int page, int pageSize)
    {
        var query = _context.LeaveRequests
            .Include(l => l.Employee)
            .Include(l => l.LeaveType)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(l => l.Status == status);
        if (!string.IsNullOrWhiteSpace(type))
            query = query.Where(l => l.LeaveType.Name == type);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(l => l.CreatedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new LeaveDto
            {
                Id = l.Id,
                EmployeeId = l.EmployeeId,
                EmployeeName = l.Employee.FirstName + " " + l.Employee.LastName,
                EmployeeCode = l.Employee.EmployeeCode,
                LeaveType = l.LeaveType.Name,
                FromDate = l.FromDate,
                ToDate = l.ToDate,
                TotalDays = l.TotalDays,
                Reason = l.Reason,
                Status = l.Status,
                AppliedOn = l.CreatedDate
            })
            .ToListAsync();

        return new LeaveListResponse { Items = items, Total = total };
    }

    public async Task<LeaveDto> GetByIdAsync(Guid id)
    {
        var leave = await _context.LeaveRequests
            .Include(l => l.Employee)
            .Include(l => l.LeaveType)
            .FirstOrDefaultAsync(l => l.Id == id)
            ?? throw new KeyNotFoundException($"Leave request with ID {id} not found");

        return new LeaveDto
        {
            Id = leave.Id,
            EmployeeId = leave.EmployeeId,
            EmployeeName = $"{leave.Employee.FirstName} {leave.Employee.LastName}",
            EmployeeCode = leave.Employee.EmployeeCode,
            LeaveType = leave.LeaveType.Name,
            FromDate = leave.FromDate,
            ToDate = leave.ToDate,
            TotalDays = leave.TotalDays,
            Reason = leave.Reason,
            Status = leave.Status,
            AppliedOn = leave.CreatedDate
        };
    }

    public async Task<LeaveDto> CreateAsync(CreateLeaveRequest request, Guid createdBy)
    {
        var totalDays = (request.ToDate - request.FromDate).Days + 1;

        var employeeId = request.EmployeeId;
        if (employeeId == Guid.Empty)
        {
            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.UserId == createdBy)
                ?? throw new InvalidOperationException("Employee profile not found");
            employeeId = employee.Id;
        }

        var balance = (await GetLeaveBalanceAsync(employeeId))
            .FirstOrDefault(b => b.LeaveTypeId == request.LeaveTypeId);

        if (balance != null && balance.Balance < totalDays)
            throw new InvalidOperationException(
                $"Insufficient leave balance. Available: {balance.Balance} {balance.Name}, Requested: {totalDays}");

        var leave = new LeaveRequest
        {
            EmployeeId = employeeId,
            LeaveTypeId = request.LeaveTypeId,
            FromDate = request.FromDate,
            ToDate = request.ToDate,
            TotalDays = totalDays,
            Reason = request.Reason,
            Status = "Pending",
            CreatedBy = createdBy,
            CreatedDate = DateTime.UtcNow
        };

        _context.LeaveRequests.Add(leave);
        await _context.SaveChangesAsync();

        await _auditService.LogAsync(EntityNames.LeaveRequest, leave.Id.ToString(), "Create", null,
            new { leave.EmployeeId, leave.LeaveTypeId, leave.FromDate, leave.ToDate, leave.TotalDays }, CurrentUserId, CurrentUserIp);

        return await GetByIdAsync(leave.Id);
    }

    public async Task<LeaveDto> UpdateAsync(Guid id, UpdateLeaveRequest request)
    {
        var leave = await _context.LeaveRequests.FindAsync(id)
            ?? throw new KeyNotFoundException($"Leave request with ID {id} not found");

        if (request.FromDate.HasValue) leave.FromDate = request.FromDate.Value;
        if (request.ToDate.HasValue) leave.ToDate = request.ToDate.Value;
        if (request.Reason != null) leave.Reason = request.Reason;

        leave.TotalDays = (leave.ToDate - leave.FromDate).Days + 1;
        leave.UpdatedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    public async Task<LeaveDto> ApproveAsync(Guid id, Guid approvedBy, string? comments)
    {
        var leave = await _context.LeaveRequests
            .Include(l => l.Employee)
            .Include(l => l.LeaveType)
            .FirstOrDefaultAsync(l => l.Id == id)
            ?? throw new KeyNotFoundException($"Leave request with ID {id} not found");

        leave.Status = "Approved";
        leave.ApprovedBy = approvedBy;
        leave.ApprovedDate = DateTime.UtcNow;
        leave.UpdatedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _auditService.LogAsync(EntityNames.LeaveRequest, leave.Id.ToString(), "Approve", null,
            new { leave.Status, leave.ApprovedBy, Comments = comments }, approvedBy, CurrentUserIp);

        await _notificationService.CreateAndSendAsync(leave.Employee.UserId, new CreateNotificationRequest
        {
            Title = "Leave Approved",
            Message = $"Your {leave.LeaveType.Name} ({leave.FromDate:dd-MMM} to {leave.ToDate:dd-MMM}) has been approved",
            Link = "/leaves"
        });

        return await GetByIdAsync(id);
    }

    public async Task<LeaveDto> RejectAsync(Guid id, Guid rejectedBy, string? comments)
    {
        var leave = await _context.LeaveRequests
            .Include(l => l.Employee)
            .Include(l => l.LeaveType)
            .FirstOrDefaultAsync(l => l.Id == id)
            ?? throw new KeyNotFoundException($"Leave request with ID {id} not found");

        leave.Status = "Rejected";
        leave.ApprovedBy = rejectedBy;
        leave.ApprovedDate = DateTime.UtcNow;
        leave.UpdatedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _auditService.LogAsync(EntityNames.LeaveRequest, leave.Id.ToString(), "Reject", null,
            new { leave.Status, leave.ApprovedBy, Comments = comments }, rejectedBy, CurrentUserIp);

        await _notificationService.CreateAndSendAsync(leave.Employee.UserId, new CreateNotificationRequest
        {
            Title = "Leave Rejected",
            Message = $"Your {leave.LeaveType.Name} ({leave.FromDate:dd-MMM} to {leave.ToDate:dd-MMM}) has been rejected" +
                      (string.IsNullOrEmpty(comments) ? "" : $". Reason: {comments}"),
            Link = "/leaves"
        });

        return await GetByIdAsync(id);
    }

    public async Task CancelAsync(Guid id)
    {
        var leave = await _context.LeaveRequests.FindAsync(id)
            ?? throw new KeyNotFoundException($"Leave request with ID {id} not found");

        leave.Status = "Cancelled";
        leave.UpdatedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        await _auditService.LogAsync(EntityNames.LeaveRequest, leave.Id.ToString(), "Cancel", null,
            new { leave.Status }, CurrentUserId, CurrentUserIp);
    }

    public async Task<List<LeaveDto>> GetByEmployeeAsync(Guid employeeId, string? status)
    {
        var query = _context.LeaveRequests
            .Include(l => l.Employee)
            .Include(l => l.LeaveType)
            .Where(l => l.EmployeeId == employeeId);

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(l => l.Status == status);

        return await query
            .OrderByDescending(l => l.CreatedDate)
            .Select(l => new LeaveDto
            {
                Id = l.Id,
                EmployeeId = l.EmployeeId,
                EmployeeName = l.Employee.FirstName + " " + l.Employee.LastName,
                EmployeeCode = l.Employee.EmployeeCode,
                LeaveType = l.LeaveType.Name,
                FromDate = l.FromDate,
                ToDate = l.ToDate,
                TotalDays = l.TotalDays,
                Reason = l.Reason,
                Status = l.Status,
                AppliedOn = l.CreatedDate
            })
            .ToListAsync();
    }

    public async Task<List<LeaveTypeDto>> GetLeaveTypesAsync()
    {
        return await _context.LeaveTypes
            .Where(lt => lt.IsActive && !lt.IsDeleted)
            .Select(lt => new LeaveTypeDto
            {
                Id = lt.Id,
                Name = lt.Name,
                DaysPerYear = lt.DaysPerYear,
                IsPaid = lt.IsPaid
            })
            .ToListAsync();
    }

    public async Task<List<LeaveBalanceDto>> GetLeaveBalanceAsync(Guid employeeId)
    {
        var leaveTypes = await _context.LeaveTypes
            .Where(lt => lt.IsActive && !lt.IsDeleted)
            .ToListAsync();

        var takenLeaves = await _context.LeaveRequests
            .Where(l => l.EmployeeId == employeeId && l.Status == "Approved")
            .GroupBy(l => l.LeaveTypeId)
            .Select(g => new { LeaveTypeId = g.Key, Total = g.Sum(l => l.TotalDays) })
            .ToListAsync();

        return leaveTypes.Select(lt => new LeaveBalanceDto
        {
            LeaveTypeId = lt.Id,
            Name = lt.Name,
            Total = lt.DaysPerYear ?? 0,
            Taken = takenLeaves.FirstOrDefault(t => t.LeaveTypeId == lt.Id)?.Total ?? 0,
            Balance = (lt.DaysPerYear ?? 0) - (takenLeaves.FirstOrDefault(t => t.LeaveTypeId == lt.Id)?.Total ?? 0),
            IsPaid = lt.IsPaid
        }).ToList();
    }
}
