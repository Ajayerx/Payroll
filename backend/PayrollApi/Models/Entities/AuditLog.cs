namespace PayrollApi.Models.Entities;

public class AuditLog
{
    public long Id { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public Guid? ChangedBy { get; set; }
    public DateTime ChangedDate { get; set; } = DateTime.UtcNow;
    public string? IpAddress { get; set; }
}
