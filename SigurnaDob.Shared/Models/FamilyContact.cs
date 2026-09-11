namespace SigurnaDob.Shared.Models;

public class FamilyContact
{
    public int Id { get; set; }
    public int ResidentId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Relationship { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }

    public Resident? Resident { get; set; }
    public AppUser? AppUser { get; set; }
    public ICollection<VisitRequest> VisitRequests { get; set; } = new List<VisitRequest>();
}
