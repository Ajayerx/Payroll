using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayrollApi.Models.DTOs;
using PayrollApi.Services;

namespace PayrollApi.Controllers;

[Route("api/v1/notifications")]
[Authorize]
public class NotificationsController : BaseApiController
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<ActionResult<NotificationListResponse>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _notificationService.GetUserNotificationsAsync(CurrentUserId, page, pageSize);
        return Ok(result);
    }

    [HttpGet("unread-count")]
    public async Task<ActionResult<int>> GetUnreadCount()
    {
        var count = await _notificationService.GetUnreadCountAsync(CurrentUserId);
        return Ok(new { count });
    }

    [HttpPut("mark-read")]
    public async Task<ActionResult> MarkAsRead([FromBody] MarkReadRequest request)
    {
        await _notificationService.MarkAsReadAsync(CurrentUserId, request);
        return NoContent();
    }

    [HttpPut("mark-all-read")]
    public async Task<ActionResult> MarkAllAsRead()
    {
        await _notificationService.MarkAllAsReadAsync(CurrentUserId);
        return NoContent();
    }

    [HttpPost]
    public async Task<ActionResult<NotificationDto>> Create([FromBody] CreateNotificationRequest request)
    {
        var result = await _notificationService.CreateAndSendAsync(CurrentUserId, request);
        return Ok(result);
    }
}
