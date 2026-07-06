using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayrollApi.Models.DTOs;
using PayrollApi.Services;

namespace PayrollApi.Controllers;

[Route("api/v1/leaves")]
[Authorize(Roles = "Admin,HRManager,Employee")]
public class LeavesController : BaseApiController
{
    private readonly ILeaveService _leaveService;

    public LeavesController(ILeaveService leaveService)
    {
        _leaveService = leaveService;
    }

    [HttpGet]
    public async Task<ActionResult<LeaveListResponse>> GetAll(
        [FromQuery] string? status, [FromQuery] string? type,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _leaveService.GetAllAsync(status, type, page, pageSize);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<LeaveDto>> GetById(Guid id)
    {
        try
        {
            var result = await _leaveService.GetByIdAsync(id);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<LeaveDto>> Create([FromBody] CreateLeaveRequest request)
    {
        var result = await _leaveService.CreateAsync(request, CurrentUserId);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<LeaveDto>> Update(Guid id, [FromBody] UpdateLeaveRequest request)
    {
        try
        {
            var result = await _leaveService.UpdateAsync(id, request);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPut("{id}/approve")]
    [Authorize(Roles = "Admin,HRManager")]
    public async Task<ActionResult<LeaveDto>> Approve(Guid id, [FromBody] ApproveLeaveRequest request)
    {
        try
        {
            var result = await _leaveService.ApproveAsync(id, CurrentUserId, request.Comments);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPut("{id}/reject")]
    [Authorize(Roles = "Admin,HRManager")]
    public async Task<ActionResult<LeaveDto>> Reject(Guid id, [FromBody] ApproveLeaveRequest request)
    {
        try
        {
            var result = await _leaveService.RejectAsync(id, CurrentUserId, request.Comments);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPut("{id}/cancel")]
    public async Task<ActionResult> Cancel(Guid id)
    {
        try
        {
            await _leaveService.CancelAsync(id);
            return Ok(new { message = "Leave request cancelled" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("employee/{employeeId}")]
    public async Task<ActionResult<List<LeaveDto>>> GetByEmployee(Guid employeeId, [FromQuery] string? status)
    {
        var result = await _leaveService.GetByEmployeeAsync(employeeId, status);
        return Ok(result);
    }

    [HttpGet("types")]
    public async Task<ActionResult<List<LeaveTypeDto>>> GetLeaveTypes()
    {
        var result = await _leaveService.GetLeaveTypesAsync();
        return Ok(result);
    }

    [HttpGet("balance/{employeeId}")]
    public async Task<ActionResult> GetLeaveBalance(Guid employeeId)
    {
        var result = await _leaveService.GetLeaveBalanceAsync(employeeId);
        return Ok(result);
    }
}
