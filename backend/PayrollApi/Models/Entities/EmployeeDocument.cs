namespace PayrollApi.Models.Entities;

public class EmployeeDocument
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string? Category { get; set; }
    public DateTime UploadedDate { get; set; } = DateTime.UtcNow;

    public Employee Employee { get; set; } = null!;
}
