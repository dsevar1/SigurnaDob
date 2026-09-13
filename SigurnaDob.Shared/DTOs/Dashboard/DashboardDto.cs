namespace SigurnaDob.Shared.DTOs.Dashboard;

public class DashboardDto
{
    // Puni agregati - popunjeni samo za Admin/Coordinator, inače null (Caregiver/FamilyMember
    // ne smiju vidjeti agregate cijelog doma, samo svoje osobne stavke - vidi PersonalCount).
    public int? ActiveResidentsCount { get; set; }
    public int? FreeRoomSlotsCount { get; set; }
    public int? OpenCareTasksCount { get; set; }
    public int? OverdueCareTasksCount { get; set; }
    public int? PendingVisitRequestsCount { get; set; }
    public List<RecentChangeDto>? RecentChanges { get; set; }

    // Osobni broj - naziv i vrijednost ovise o ulozi prijavljenog korisnika.
    // FamilyMember bez povezanog profila: PersonalCount = null ("račun nije povezan"), ne 0
    // ("povezan si, ali nemaš nijedan na čekanju") - namjerna razlika.
    public string? PersonalCountLabel { get; set; }
    public int? PersonalCount { get; set; }
}
