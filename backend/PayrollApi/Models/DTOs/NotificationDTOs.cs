namespace PayrollApi.Models.DTOs;

public class NotificationDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Link { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class NotificationListResponse
{
    public List<NotificationDto> Items { get; set; } = new();
    public int UnreadCount { get; set; }
    public int Total { get; set; }
}

public class MarkReadRequest
{
    public List<Guid> Ids { get; set; } = new();
}

public class CreateNotificationRequest
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Link { get; set; }
}
