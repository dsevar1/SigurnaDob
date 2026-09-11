namespace SigurnaDob.Shared.Models;

public class Staff
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Position { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public bool IsActive { get; set; } = true;

    public AppUser? AppUser { get; set; }
    public ICollection<CareTask> AssignedCareTasks { get; set; } = new List<CareTask>();
    public ICollection<VisitRequest> DecidedVisitRequests { get; set; } = new List<VisitRequest>();
}
