namespace SigurnaDob.Shared.Models;

public class ActivityParticipant
{
    public int ActivityId { get; set; }
    public int ResidentId { get; set; }

    public Activity? Activity { get; set; }
    public Resident? Resident { get; set; }
}
