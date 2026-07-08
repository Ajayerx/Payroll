namespace PayrollApi.Models.DTOs;

public class EmployeeDocumentDto
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string? Category { get; set; }
    public DateTime UploadedDate { get; set; }
}

public class BulkImportResult
{
    public int Imported { get; set; }
    public int Failed { get; set; }
    public List<string> Errors { get; set; } = new();
}
