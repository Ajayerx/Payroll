using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayrollApi.Models.DTOs;
using PayrollApi.Services;

namespace PayrollApi.Controllers;

[Route("api/v1/employees/{employeeId}/documents")]
[Authorize(Roles = "Admin,HRManager")]
public class DocumentsController : BaseApiController
{
    private readonly IEmployeeService _employeeService;

    public DocumentsController(IEmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    [HttpGet]
    public async Task<ActionResult<List<EmployeeDocumentDto>>> GetAll(Guid employeeId)
    {
        var result = await _employeeService.GetDocumentsAsync(employeeId);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<EmployeeDocumentDto>> Upload(Guid employeeId, IFormFile file, [FromQuery] string? category)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "No file provided" });
        var result = await _employeeService.UploadDocumentAsync(employeeId, file, category);
        return Ok(result);
    }

    [HttpDelete("{documentId}")]
    public async Task<ActionResult> Delete(Guid employeeId, Guid documentId)
    {
        try
        {
            await _employeeService.DeleteDocumentAsync(employeeId, documentId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
