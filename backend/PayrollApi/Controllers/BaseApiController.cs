using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace PayrollApi.Controllers;

[ApiController]
public abstract class BaseApiController : ControllerBase
{
    protected Guid CurrentUserId =>
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : Guid.Empty;

    protected string CurrentRole =>
        User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

    protected string CurrentUserEmail =>
        User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;

    protected string? CurrentUserIp =>
        HttpContext.Connection.RemoteIpAddress?.ToString();

    protected bool IsAuthenticated => User.Identity?.IsAuthenticated ?? false;
}
