using System.Text.Json;
using PayrollApi.Data;
using PayrollApi.Models.Entities;
using PayrollApi.Services.Interfaces;

namespace PayrollApi.Services;

public class AuditService : IAuditService
{
    private readonly PayrollDbContext _context;

    public AuditService(PayrollDbContext context)
    {
        _context = context;
    }

    public async Task LogAsync<T>(string entityName, string entityId, string action, T? oldEntity, T? newEntity, Guid? changedBy, string? ipAddress = null)
    {
        var audit = new AuditLog
        {
            EntityName = entityName,
            EntityId = entityId,
            Action = action,
            OldValues = oldEntity is not null ? JsonSerializer.Serialize(oldEntity) : null,
            NewValues = newEntity is not null ? JsonSerializer.Serialize(newEntity) : null,
            ChangedBy = changedBy,
            ChangedDate = DateTime.UtcNow,
            IpAddress = ipAddress
        };

        _context.AuditLogs.Add(audit);
        await _context.SaveChangesAsync();
    }
}
