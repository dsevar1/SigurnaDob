namespace SigurnaDob.Shared.DTOs.VisitRequests;

public class VisitRequestDto
{
    public int Id { get; set; }
    public int ResidentId { get; set; }
    public string ResidentFullName { get; set; } = string.Empty;
    public int FamilyContactId { get; set; }
    public string FamilyContactFullName { get; set; } = string.Empty;
    public int VisitRequestStatusId { get; set; }
    public string VisitRequestStatusName { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; }
    public int? DecidedByStaffId { get; set; }
    public string? DecidedByStaffFullName { get; set; }
    public string? DecisionNote { get; set; }
    public DateTime CreatedAt { get; set; }
}
