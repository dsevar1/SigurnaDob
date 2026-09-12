namespace SigurnaDob.Shared.DTOs.ResidentDocuments;

public class ResidentDocumentDto
{
    public int Id { get; set; }
    public int ResidentId { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public DateTime UploadedAt { get; set; }
}
