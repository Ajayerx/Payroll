namespace PayrollApi.Models.Entities;

public class Notification
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Link { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
}
