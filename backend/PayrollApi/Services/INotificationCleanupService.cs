namespace PayrollApi.Services;

public interface INotificationCleanupService
{
    Task CleanupOldNotificationsAsync();
}
