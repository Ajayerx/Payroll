using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PayrollApi.Data;
using PayrollApi.Hubs;
using PayrollApi.Models.DTOs;
using PayrollApi.Models.Entities;

namespace PayrollApi.Services;

public class NotificationService : INotificationService
{
    private readonly PayrollDbContext _context;
    private readonly IHubContext<NotificationHub> _hub;

    public NotificationService(PayrollDbContext context, IHubContext<NotificationHub> hub)
    {
        _context = context;
        _hub = hub;
    }

    public async Task<NotificationListResponse> GetUserNotificationsAsync(Guid userId, int page = 1, int pageSize = 20)
    {
        var query = _context.Notifications.Where(n => n.UserId == userId);
        var total = await query.CountAsync();
        var unreadCount = await query.CountAsync(n => !n.IsRead);

        var items = await query
            .OrderByDescending(n => n.CreatedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(n => new NotificationDto
            {
                Id = n.Id,
                Title = n.Title,
                Message = n.Message,
                Link = n.Link,
                IsRead = n.IsRead,
                CreatedDate = n.CreatedDate
            })
            .ToListAsync();

        return new NotificationListResponse
        {
            Items = items,
            UnreadCount = unreadCount,
            Total = total
        };
    }

    public async Task MarkAsReadAsync(Guid userId, MarkReadRequest request)
    {
        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId && request.Ids.Contains(n.Id))
            .ToListAsync();

        foreach (var n in notifications)
            n.IsRead = true;

        await _context.SaveChangesAsync();
    }

    public async Task MarkAllAsReadAsync(Guid userId)
    {
        await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ExecuteUpdateAsync(setters => setters.SetProperty(n => n.IsRead, true));
    }

    public async Task<int> GetUnreadCountAsync(Guid userId)
    {
        return await _context.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead);
    }

    public async Task<NotificationDto> CreateAndSendAsync(Guid userId, CreateNotificationRequest request)
    {
        var notification = new Notification
        {
            UserId = userId,
            Title = request.Title,
            Message = request.Message,
            Link = request.Link
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        await _hub.Clients.Group($"user_{userId}").SendAsync("ReceiveNotification", new
        {
            Id = notification.Id,
            Title = notification.Title,
            Message = notification.Message,
            Link = notification.Link,
            IsRead = notification.IsRead,
            Timestamp = notification.CreatedDate
        });

        return new NotificationDto
        {
            Id = notification.Id,
            Title = notification.Title,
            Message = notification.Message,
            Link = notification.Link,
            IsRead = notification.IsRead,
            CreatedDate = notification.CreatedDate
        };
    }

    public async Task DeleteOldNotificationsAsync(int daysOld = 30)
    {
        var cutoff = DateTime.UtcNow.AddDays(-daysOld);
        await _context.Notifications
            .Where(n => n.CreatedDate < cutoff)
            .ExecuteDeleteAsync();
    }
}
