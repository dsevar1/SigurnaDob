namespace SigurnaDob.Shared.Models;

public class ResidentDocument
{
    public int Id { get; set; }
    public int ResidentId { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public DateTime UploadedAt { get; set; }

    public Resident? Resident { get; set; }
}
