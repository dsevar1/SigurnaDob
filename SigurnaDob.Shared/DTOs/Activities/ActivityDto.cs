namespace SigurnaDob.Shared.DTOs.Activities;

public class ActivityDto
{
    public int Id { get; set; }
    public int ActivityTypeId { get; set; }
    public string ActivityTypeName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime ScheduledAt { get; set; }
    public int ParticipantCount { get; set; }
    public List<ActivityParticipantDto> Participants { get; set; } = new();
}
