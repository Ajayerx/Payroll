namespace PayrollApi.Services;

public class NotificationCleanupHostedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<NotificationCleanupHostedService> _logger;

    public NotificationCleanupHostedService(IServiceProvider serviceProvider, ILogger<NotificationCleanupHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var cleanupService = scope.ServiceProvider.GetRequiredService<INotificationCleanupService>();
                await cleanupService.CleanupOldNotificationsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Notification cleanup failed");
            }

            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
        }
    }
}
