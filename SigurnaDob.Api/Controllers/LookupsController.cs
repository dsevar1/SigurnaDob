using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SigurnaDob.Api.Data;
using SigurnaDob.Shared.DTOs.Lookups;

namespace SigurnaDob.Api.Controllers;

[ApiController]
[Route("api/lookups")]
[Authorize]
public class LookupsController : ControllerBase
{
    private readonly SigurnaDobDbContext _db;

    public LookupsController(SigurnaDobDbContext db)
    {
        _db = db;
    }

    [HttpGet("room-statuses")]
    public async Task<ActionResult<List<LookupDto>>> GetRoomStatuses()
    {
        var statuses = await _db.RoomStatuses
            .OrderBy(s => s.Id)
            .Select(s => new LookupDto { Id = s.Id, Name = s.Name })
            .ToListAsync();

        return Ok(statuses);
    }

    [HttpGet("resident-statuses")]
    public async Task<ActionResult<List<LookupDto>>> GetResidentStatuses()
    {
        var statuses = await _db.ResidentStatuses
            .OrderBy(s => s.Id)
            .Select(s => new LookupDto { Id = s.Id, Name = s.Name })
            .ToListAsync();

        return Ok(statuses);
    }

    [HttpGet("care-task-statuses")]
    public async Task<ActionResult<List<LookupDto>>> GetCareTaskStatuses()
    {
        var statuses = await _db.CareTaskStatuses
            .OrderBy(s => s.Id)
            .Select(s => new LookupDto { Id = s.Id, Name = s.Name })
            .ToListAsync();

        return Ok(statuses);
    }

    [HttpGet("care-task-types")]
    public async Task<ActionResult<List<LookupDto>>> GetCareTaskTypes()
    {
        var types = await _db.CareTaskTypes
            .OrderBy(t => t.Id)
            .Select(t => new LookupDto { Id = t.Id, Name = t.Name })
            .ToListAsync();

        return Ok(types);
    }

    [HttpGet("visit-request-statuses")]
    public async Task<ActionResult<List<LookupDto>>> GetVisitRequestStatuses()
    {
        var statuses = await _db.VisitRequestStatuses
            .OrderBy(s => s.Id)
            .Select(s => new LookupDto { Id = s.Id, Name = s.Name })
            .ToListAsync();

        return Ok(statuses);
    }

    [HttpGet("activity-types")]
    public async Task<ActionResult<List<LookupDto>>> GetActivityTypes()
    {
        var types = await _db.ActivityTypes
            .OrderBy(t => t.Id)
            .Select(t => new LookupDto { Id = t.Id, Name = t.Name })
            .ToListAsync();

        return Ok(types);
    }

    [HttpGet("staff")]
    public async Task<ActionResult<List<LookupDto>>> GetStaff()
    {
        var staff = await _db.Staff
            .Where(s => s.IsActive)
            .OrderBy(s => s.FullName)
            .Select(s => new LookupDto { Id = s.Id, Name = s.FullName })
            .ToListAsync();

        return Ok(staff);
    }
}
