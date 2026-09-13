namespace SigurnaDob.Shared.DTOs.Overview;

public class CalendarEntryDto
{
    public DateTime OccursAt { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
