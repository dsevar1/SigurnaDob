namespace SigurnaDob.Shared.DTOs.Activities;

public class ActivityUpdateDto
{
    public int ActivityTypeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime ScheduledAt { get; set; }
}
