using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SigurnaDob.Api.Data;
using SigurnaDob.Api.Services;
using SigurnaDob.Shared.Constants;
using SigurnaDob.Shared.DTOs.Dashboard;

namespace SigurnaDob.Api.Controllers;

// [Authorize] bez Roles: svaka prijavljena uloga smije pozvati endpoint, grananje na puni/osobni
// prikaz ide INSIDE metode preko User.IsInRole - nema class-level Roles atributa koji bi se
// sudarao s nečim drugim (nema AND-trap rizika jer nema method-level [Authorize(Roles=...)]).
[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardController : ControllerBase
{
    private static readonly int[] OpenCareTaskStatuses =
        [CareTaskStatusIds.Novo, CareTaskStatusIds.Dodijeljeno, CareTaskStatusIds.UTijeku];

    private static readonly int[] ClosedCareTaskStatuses =
        [CareTaskStatusIds.Izvrseno, CareTaskStatusIds.Otkazano];

    private readonly SigurnaDobDbContext _db;
    private readonly RoomOccupancyService _roomOccupancy;

    public DashboardController(SigurnaDobDbContext db, RoomOccupancyService roomOccupancy)
    {
        _db = db;
        _roomOccupancy = roomOccupancy;
    }

    [HttpGet]
    public async Task<ActionResult<DashboardDto>> GetDashboard()
    {
        var dto = new DashboardDto();

        if (User.IsInRole("Admin") || User.IsInRole("Coordinator"))
        {
            await PopulateFullAggregatesAsync(dto);
        }
        else if (User.IsInRole("Caregiver"))
        {
            await PopulateCaregiverPersonalCountAsync(dto);
        }
        else if (User.IsInRole("FamilyMember"))
        {
            await PopulateFamilyMemberPersonalCountAsync(dto);
        }

        return Ok(dto);
    }

    private async Task PopulateFullAggregatesAsync(DashboardDto dto)
    {
        dto.ActiveResidentsCount = await _db.Residents.CountAsync(r => r.ResidentStatusId == ResidentStatusIds.Aktivan);

        var usableRooms = await _db.Rooms.Where(r => r.RoomStatusId == RoomStatusIds.UUporabi).ToListAsync();
        var occupancyByRoom = await _roomOccupancy.GetOccupancyByRoomAsync();
        dto.FreeRoomSlotsCount = usableRooms.Sum(r => Math.Max(0, r.Capacity - occupancyByRoom.GetValueOrDefault(r.Id)));

        dto.OpenCareTasksCount = await _db.CareTasks.CountAsync(t => OpenCareTaskStatuses.Contains(t.CareTaskStatusId));

        var utcNow = DateTime.UtcNow;
        dto.OverdueCareTasksCount = await _db.CareTasks
            .CountAsync(t => t.DueDate < utcNow && !ClosedCareTaskStatuses.Contains(t.CareTaskStatusId));

        dto.PendingVisitRequestsCount = await _db.VisitRequests
            .CountAsync(v => v.VisitRequestStatusId == VisitRequestStatusIds.Zaprimljeno);

        dto.RecentChanges = await GetRecentChangesAsync();
    }

    private async Task PopulateCaregiverPersonalCountAsync(DashboardDto dto)
    {
        var staffId = GetStaffIdFromClaims();
        dto.PersonalCountLabel = "Moji otvoreni zadaci";

        if (staffId is null)
        {
            return;
        }

        dto.PersonalCount = await _db.CareTasks
            .CountAsync(t => t.AssignedStaffId == staffId && OpenCareTaskStatuses.Contains(t.CareTaskStatusId));
    }

    private async Task PopulateFamilyMemberPersonalCountAsync(DashboardDto dto)
    {
        var familyContactId = GetFamilyContactIdFromClaims();
        dto.PersonalCountLabel = "Moji zahtjevi na čekanju";

        if (familyContactId is null)
        {
            return;
        }

        dto.PersonalCount = await _db.VisitRequests
            .CountAsync(v => v.FamilyContactId == familyContactId && v.VisitRequestStatusId == VisitRequestStatusIds.Zaprimljeno);
    }

    private async Task<List<RecentChangeDto>> GetRecentChangesAsync()
    {
        var changes = new List<RecentChangeDto>();

        var recentCreatedTasks = await _db.CareTasks
            .Include(t => t.Resident)
            .Include(t => t.CareTaskType)
            .OrderByDescending(t => t.CreatedAt)
            .Take(5)
            .ToListAsync();
        changes.AddRange(recentCreatedTasks.Select(t => new RecentChangeDto
        {
            OccurredAt = t.CreatedAt,
            Description = $"Kreiran zadatak: {t.CareTaskType?.Name} za {t.Resident?.FullName}"
        }));

        var recentCompletedTasks = await _db.CareTasks
            .Include(t => t.Resident)
            .Include(t => t.CareTaskType)
            .Where(t => t.CompletedAt != null)
            .OrderByDescending(t => t.CompletedAt)
            .Take(5)
            .ToListAsync();
        changes.AddRange(recentCompletedTasks.Select(t => new RecentChangeDto
        {
            OccurredAt = t.CompletedAt!.Value,
            Description = $"Završen zadatak: {t.CareTaskType?.Name} za {t.Resident?.FullName}"
        }));

        var recentVisitRequests = await _db.VisitRequests
            .Include(v => v.Resident)
            .Include(v => v.FamilyContact)
            .OrderByDescending(v => v.CreatedAt)
            .Take(5)
            .ToListAsync();
        changes.AddRange(recentVisitRequests.Select(v => new RecentChangeDto
        {
            OccurredAt = v.CreatedAt,
            Description = $"Zaprimljen zahtjev za posjet: {v.Resident?.FullName} ({v.FamilyContact?.FullName})"
        }));

        var recentDocuments = await _db.ResidentDocuments
            .Include(d => d.Resident)
            .OrderByDescending(d => d.UploadedAt)
            .Take(5)
            .ToListAsync();
        changes.AddRange(recentDocuments.Select(d => new RecentChangeDto
        {
            OccurredAt = d.UploadedAt,
            Description = $"Dodan dokument: {d.OriginalFileName} ({d.Resident?.FullName})"
        }));

        return changes.OrderByDescending(c => c.OccurredAt).Take(5).ToList();
    }

    private int? GetStaffIdFromClaims()
    {
        var claim = User.FindFirst("staff_id");
        if (claim is null || !int.TryParse(claim.Value, out var staffId))
        {
            return null;
        }

        return staffId;
    }

    private int? GetFamilyContactIdFromClaims()
    {
        var claim = User.FindFirst("family_contact_id");
        if (claim is null || !int.TryParse(claim.Value, out var familyContactId))
        {
            return null;
        }

        return familyContactId;
    }
}
