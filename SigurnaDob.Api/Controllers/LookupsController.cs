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
}
