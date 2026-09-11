namespace SigurnaDob.Shared.Models;

public class VisitRequest
{
    public int Id { get; set; }
    public int ResidentId { get; set; }
    public int FamilyContactId { get; set; }
    public int VisitRequestStatusId { get; set; }
    public DateTime RequestedAt { get; set; }
    public int? DecidedByStaffId { get; set; }
    public string? DecisionNote { get; set; }
    public DateTime CreatedAt { get; set; }

    public Resident? Resident { get; set; }
    public FamilyContact? FamilyContact { get; set; }
    public VisitRequestStatus? VisitRequestStatus { get; set; }
    public Staff? DecidedByStaff { get; set; }
}
