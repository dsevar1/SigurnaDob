namespace SigurnaDob.Shared.DTOs.CareTasks;

public class CareTaskCompleteDto
{
    public DateTime CompletedAt { get; set; }
    public string CompletionNote { get; set; } = string.Empty;
}
