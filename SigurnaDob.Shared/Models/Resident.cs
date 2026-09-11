namespace SigurnaDob.Shared.Models;

public class Resident
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public DateOnly? DateOfBirth { get; set; }
    public int ResidentStatusId { get; set; }
    public int? RoomId { get; set; }
    public DateOnly? AdmissionDate { get; set; }
    public DateTime CreatedAt { get; set; }

    public ResidentStatus? ResidentStatus { get; set; }
    public Room? Room { get; set; }
    public ICollection<FamilyContact> FamilyContacts { get; set; } = new List<FamilyContact>();
    public ICollection<CareTask> CareTasks { get; set; } = new List<CareTask>();
    public ICollection<VisitRequest> VisitRequests { get; set; } = new List<VisitRequest>();
    public ICollection<ActivityParticipant> ActivityParticipants { get; set; } = new List<ActivityParticipant>();
    public ICollection<ResidentDocument> ResidentDocuments { get; set; } = new List<ResidentDocument>();
}
