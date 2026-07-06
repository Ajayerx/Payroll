using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayrollApi.Models.DTOs;
using PayrollApi.Services;

namespace PayrollApi.Controllers;

[Route("api/v1/settings")]
[Authorize(Roles = "Admin")]
public class SettingsController : BaseApiController
{
    private readonly ISettingsService _settingsService;

    public SettingsController(ISettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    [HttpGet("company")]
    public async Task<ActionResult<CompanySettingDto>> GetCompany()
    {
        var result = await _settingsService.GetCompanyAsync();
        return Ok(result);
    }

    [HttpPut("company")]
    public async Task<ActionResult<CompanySettingDto>> UpdateCompany([FromBody] UpdateCompanySettingRequest request)
    {
        var result = await _settingsService.UpdateCompanyAsync(request);
        return Ok(result);
    }

    [HttpGet("tax-slabs")]
    public async Task<ActionResult<List<TaxSlabDto>>> GetTaxSlabs()
    {
        var result = await _settingsService.GetTaxSlabsAsync();
        return Ok(result);
    }

    [HttpPost("tax-slabs")]
    public async Task<ActionResult<TaxSlabDto>> CreateTaxSlab([FromBody] CreateTaxSlabRequest request)
    {
        var result = await _settingsService.CreateTaxSlabAsync(request);
        return CreatedAtAction(nameof(GetTaxSlabs), null, result);
    }

    [HttpGet("leave-types")]
    public async Task<ActionResult<List<LeaveTypeDto>>> GetLeaveTypes()
    {
        var result = await _settingsService.GetLeaveTypesAsync();
        return Ok(result);
    }

    [HttpPost("leave-types")]
    public async Task<ActionResult<LeaveTypeDto>> CreateLeaveType([FromBody] CreateLeaveTypeRequest request)
    {
        var result = await _settingsService.CreateLeaveTypeAsync(request);
        return CreatedAtAction(nameof(GetLeaveTypes), null, result);
    }
}
