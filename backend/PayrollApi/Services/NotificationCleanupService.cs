namespace PayrollApi.Services;

public class NotificationCleanupService : INotificationCleanupService
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<NotificationCleanupService> _logger;

    public NotificationCleanupService(INotificationService notificationService, ILogger<NotificationCleanupService> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task CleanupOldNotificationsAsync()
    {
        try
        {
            await _notificationService.DeleteOldNotificationsAsync(30);
            _logger.LogInformation("Old notifications cleaned up successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clean up old notifications");
        }
    }
}
