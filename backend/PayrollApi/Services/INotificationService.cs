using PayrollApi.Models.DTOs;

namespace PayrollApi.Services;

public interface INotificationService
{
    Task<NotificationListResponse> GetUserNotificationsAsync(Guid userId, int page = 1, int pageSize = 20);
    Task MarkAsReadAsync(Guid userId, MarkReadRequest request);
    Task MarkAllAsReadAsync(Guid userId);
    Task<int> GetUnreadCountAsync(Guid userId);
    Task<NotificationDto> CreateAndSendAsync(Guid userId, CreateNotificationRequest request);
    Task DeleteOldNotificationsAsync(int daysOld = 30);
}
