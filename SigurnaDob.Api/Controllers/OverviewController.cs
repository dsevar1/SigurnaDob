using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SigurnaDob.Api.Data;
using SigurnaDob.Api.Services;
using SigurnaDob.Shared.Constants;
using SigurnaDob.Shared.DTOs.Overview;

namespace SigurnaDob.Api.Controllers;

// Operativni pregled popunjenosti soba i opterećenja njegovatelja - isti krug korisnika kao puni
// Dashboard agregati (Admin/Coordinator), zato je class-level [Authorize(Roles=...)] siguran (sve
// akcije dijele istu ulogu, nema AND-trap rizika opisanog u CLAUDE.md).
[ApiController]
[Route("api/overview")]
[Authorize(Roles = "Admin,Coordinator")]
public class OverviewController : ControllerBase
{
    private static readonly int[] OpenCareTaskStatuses =
        [CareTaskStatusIds.Novo, CareTaskStatusIds.Dodijeljeno, CareTaskStatusIds.UTijeku];

    private readonly SigurnaDobDbContext _db;
    private readonly RoomOccupancyService _roomOccupancy;

    public OverviewController(SigurnaDobDbContext db, RoomOccupancyService roomOccupancy)
    {
        _db = db;
        _roomOccupancy = roomOccupancy;
    }

    [HttpGet("rooms")]
    public async Task<ActionResult<List<RoomOverviewDto>>> GetRoomsOverview()
    {
        var rooms = await _db.Rooms
            .Include(r => r.RoomStatus)
            .OrderBy(r => r.RoomNumber)
            .ToListAsync();

        var occupancyByRoom = await _roomOccupancy.GetOccupancyByRoomAsync();

        var result = rooms.Select(r =>
        {
            var occupancy = occupancyByRoom.GetValueOrDefault(r.Id, 0);
            return new RoomOverviewDto
            {
                Id = r.Id,
                RoomNumber = r.RoomNumber,
                Capacity = r.Capacity,
                CurrentOccupancy = occupancy,
                FreeSlots = r.Capacity - occupancy,
                RoomStatusName = r.RoomStatus?.Name ?? string.Empty
            };
        }).ToList();

        return Ok(result);
    }

    [HttpGet("staff-workload")]
    public async Task<ActionResult<List<StaffWorkloadDto>>> GetStaffWorkload()
    {
        var staff = await _db.Staff
            .Where(s => s.IsActive)
            .OrderBy(s => s.FullName)
            .ToListAsync();

        var openTaskCountsByStaff = await _db.CareTasks
            .Where(t => t.AssignedStaffId != null && OpenCareTaskStatuses.Contains(t.CareTaskStatusId))
            .GroupBy(t => t.AssignedStaffId!.Value)
            .Select(g => new { StaffId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.StaffId, x => x.Count);

        var result = staff.Select(s => new StaffWorkloadDto
        {
            Id = s.Id,
            StaffFullName = s.FullName,
            OpenCareTasksCount = openTaskCountsByStaff.GetValueOrDefault(s.Id, 0)
        }).ToList();

        return Ok(result);
    }

    [HttpGet("calendar")]
    public async Task<ActionResult<List<CalendarEntryDto>>> GetCalendar()
    {
        var today = DateTime.UtcNow.Date;
        var entries = new List<CalendarEntryDto>();

        var upcomingVisits = await _db.VisitRequests
            .Include(v => v.Resident)
            .Include(v => v.FamilyContact)
            .Where(v => v.VisitRequestStatusId == VisitRequestStatusIds.Odobreno && v.RequestedAt >= today)
            .ToListAsync();
        entries.AddRange(upcomingVisits.Select(v => new CalendarEntryDto
        {
            OccursAt = v.RequestedAt,
            Type = "Posjet",
            Description = $"Posjet: {v.Resident?.FullName} - {v.FamilyContact?.FullName}"
        }));

        var upcomingActivities = await _db.Activities
            .Include(a => a.ActivityType)
            .Where(a => a.ScheduledAt >= today)
            .ToListAsync();
        entries.AddRange(upcomingActivities.Select(a => new CalendarEntryDto
        {
            OccursAt = a.ScheduledAt,
            Type = "Aktivnost",
            Description = $"{a.ActivityType?.Name}: {a.Name}"
        }));

        var result = entries.OrderBy(e => e.OccursAt).ToList();
        return Ok(result);
    }
}
