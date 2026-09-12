using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SigurnaDob.Api.Data;
using SigurnaDob.Shared.DTOs.Activities;
using SigurnaDob.Shared.Models;

namespace SigurnaDob.Api.Controllers;

// Class-level [Authorize] je ovdje sigurno (za razliku od CareTasks/VisitRequests) jer SVE
// akcije - uključujući upravljanje sudionicima - trebaju samo Admin/Coordinator; nema
// mixed-role akcije (nema /mine ekvivalenta), pa nema AND-kombinacija rizika.
[ApiController]
[Route("api/activities")]
[Authorize(Roles = "Admin,Coordinator")]
public class ActivitiesController : ControllerBase
{
    private readonly SigurnaDobDbContext _db;

    public ActivitiesController(SigurnaDobDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<List<ActivityDto>>> GetActivities(
        [FromQuery] int? activityTypeId,
        [FromQuery] string? sortBy,
        [FromQuery] bool sortDescending = false)
    {
        var query = _db.Activities
            .Include(a => a.ActivityType)
            .Include(a => a.ActivityParticipants).ThenInclude(ap => ap.Resident)
            .AsQueryable();

        if (activityTypeId is not null)
        {
            query = query.Where(a => a.ActivityTypeId == activityTypeId);
        }

        query = sortBy?.ToLowerInvariant() switch
        {
            "name" => sortDescending ? query.OrderByDescending(a => a.Name) : query.OrderBy(a => a.Name),
            _ => sortDescending ? query.OrderByDescending(a => a.ScheduledAt) : query.OrderBy(a => a.ScheduledAt)
        };

        var activities = await query.ToListAsync();
        return Ok(activities.Select(MapToDto).ToList());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ActivityDto>> GetActivityById(int id)
    {
        var activity = await LoadActivityAsync(id);
        if (activity is null)
        {
            return NotFound();
        }

        return Ok(MapToDto(activity));
    }

    [HttpPost]
    public async Task<ActionResult<ActivityDto>> CreateActivity(ActivityCreateDto dto)
    {
        var validationError = await ValidateAsync(dto.ActivityTypeId, dto.Name, dto.ScheduledAt);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        var activity = new Activity
        {
            ActivityTypeId = dto.ActivityTypeId,
            Name = dto.Name,
            Description = dto.Description,
            ScheduledAt = dto.ScheduledAt
        };

        _db.Activities.Add(activity);
        await _db.SaveChangesAsync();

        await _db.Entry(activity).Reference(a => a.ActivityType).LoadAsync();

        return CreatedAtAction(nameof(GetActivityById), new { id = activity.Id }, MapToDto(activity));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateActivity(int id, ActivityUpdateDto dto)
    {
        var activity = await _db.Activities.FirstOrDefaultAsync(a => a.Id == id);
        if (activity is null)
        {
            return NotFound();
        }

        var validationError = await ValidateAsync(dto.ActivityTypeId, dto.Name, dto.ScheduledAt);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        activity.ActivityTypeId = dto.ActivityTypeId;
        activity.Name = dto.Name;
        activity.Description = dto.Description;
        activity.ScheduledAt = dto.ScheduledAt;

        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("{id:int}/participants")]
    public async Task<ActionResult<ActivityDto>> AddParticipant(int id, ActivityAddParticipantDto dto)
    {
        var activity = await LoadActivityAsync(id);
        if (activity is null)
        {
            return NotFound();
        }

        var residentExists = await _db.Residents.AnyAsync(r => r.Id == dto.ResidentId);
        if (!residentExists)
        {
            return NotFound("Nepostojeći korisnik doma.");
        }

        if (activity.ActivityParticipants.Any(ap => ap.ResidentId == dto.ResidentId))
        {
            return BadRequest("Korisnik doma već sudjeluje u ovoj aktivnosti.");
        }

        _db.ActivityParticipants.Add(new ActivityParticipant { ActivityId = id, ResidentId = dto.ResidentId });
        await _db.SaveChangesAsync();

        var reloaded = await LoadActivityAsync(id);
        return Ok(MapToDto(reloaded!));
    }

    [HttpDelete("{id:int}/participants/{residentId:int}")]
    public async Task<IActionResult> RemoveParticipant(int id, int residentId)
    {
        var participant = await _db.ActivityParticipants
            .FirstOrDefaultAsync(ap => ap.ActivityId == id && ap.ResidentId == residentId);

        if (participant is null)
        {
            return NotFound();
        }

        _db.ActivityParticipants.Remove(participant);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    private async Task<Activity?> LoadActivityAsync(int id)
    {
        return await _db.Activities
            .Include(a => a.ActivityType)
            .Include(a => a.ActivityParticipants).ThenInclude(ap => ap.Resident)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    private async Task<string?> ValidateAsync(int activityTypeId, string name, DateTime scheduledAt)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return "Naziv aktivnosti je obavezan.";
        }

        var typeExists = await _db.ActivityTypes.AnyAsync(t => t.Id == activityTypeId);
        if (!typeExists)
        {
            return "Nepostojeći tip aktivnosti.";
        }

        if (scheduledAt == default)
        {
            return "Termin aktivnosti je obavezan.";
        }

        return null;
    }

    private static ActivityDto MapToDto(Activity activity)
    {
        var participants = activity.ActivityParticipants
            .Select(ap => new ActivityParticipantDto
            {
                ResidentId = ap.ResidentId,
                ResidentFullName = ap.Resident?.FullName ?? string.Empty
            })
            .ToList();

        return new ActivityDto
        {
            Id = activity.Id,
            ActivityTypeId = activity.ActivityTypeId,
            ActivityTypeName = activity.ActivityType?.Name ?? string.Empty,
            Name = activity.Name,
            Description = activity.Description,
            ScheduledAt = activity.ScheduledAt,
            ParticipantCount = participants.Count,
            Participants = participants
        };
    }
}
