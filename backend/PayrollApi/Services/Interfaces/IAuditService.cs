namespace PayrollApi.Services.Interfaces;

public interface IAuditService
{
    Task LogAsync<T>(string entityName, string entityId, string action, T? oldEntity, T? newEntity, Guid? changedBy, string? ipAddress = null);
}
