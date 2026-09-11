namespace SigurnaDob.Shared.Models;

public class Activity
{
    public int Id { get; set; }
    public int ActivityTypeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime ScheduledAt { get; set; }

    public ActivityType? ActivityType { get; set; }
    public ICollection<ActivityParticipant> ActivityParticipants { get; set; } = new List<ActivityParticipant>();
}
