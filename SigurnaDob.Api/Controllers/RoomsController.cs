using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SigurnaDob.Api.Data;
using SigurnaDob.Shared.Constants;
using SigurnaDob.Shared.DTOs.Rooms;
using SigurnaDob.Shared.Models;

namespace SigurnaDob.Api.Controllers;

[ApiController]
[Route("api/rooms")]
[Authorize]
public class RoomsController : ControllerBase
{
    private readonly SigurnaDobDbContext _db;

    public RoomsController(SigurnaDobDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<List<RoomDto>>> GetRooms(
        [FromQuery] string? search,
        [FromQuery] int? roomStatusId,
        [FromQuery] int? minCapacity,
        [FromQuery] int? maxCapacity,
        [FromQuery] string? sortBy,
        [FromQuery] bool sortDescending = false)
    {
        var query = _db.Rooms.Include(r => r.RoomStatus).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(r => r.RoomNumber.Contains(search));
        }

        if (roomStatusId is not null)
        {
            query = query.Where(r => r.RoomStatusId == roomStatusId);
        }

        if (minCapacity is not null)
        {
            query = query.Where(r => r.Capacity >= minCapacity);
        }

        if (maxCapacity is not null)
        {
            query = query.Where(r => r.Capacity <= maxCapacity);
        }

        query = sortBy?.ToLowerInvariant() switch
        {
            "capacity" => sortDescending ? query.OrderByDescending(r => r.Capacity) : query.OrderBy(r => r.Capacity),
            "status" => sortDescending ? query.OrderByDescending(r => r.RoomStatus!.Name) : query.OrderBy(r => r.RoomStatus!.Name),
            _ => sortDescending ? query.OrderByDescending(r => r.RoomNumber) : query.OrderBy(r => r.RoomNumber)
        };

        var rooms = await query.ToListAsync();
        var occupancyByRoom = await GetOccupancyByRoomAsync();

        var result = rooms.Select(r => MapToDto(r, occupancyByRoom.GetValueOrDefault(r.Id, 0))).ToList();
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<RoomDto>> GetRoomById(int id)
    {
        var room = await _db.Rooms.Include(r => r.RoomStatus).FirstOrDefaultAsync(r => r.Id == id);
        if (room is null)
        {
            return NotFound();
        }

        var occupancy = await GetOccupancyForRoomAsync(id);
        return Ok(MapToDto(room, occupancy));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Coordinator")]
    public async Task<ActionResult<RoomDto>> CreateRoom(RoomCreateDto dto)
    {
        if (dto.Capacity <= 0)
        {
            return BadRequest("Kapacitet sobe mora biti veći od nule.");
        }

        var statusExists = await _db.RoomStatuses.AnyAsync(s => s.Id == dto.RoomStatusId);
        if (!statusExists)
        {
            return BadRequest("Nepostojeći status sobe.");
        }

        var numberTaken = await _db.Rooms.AnyAsync(r => r.RoomNumber == dto.RoomNumber);
        if (numberTaken)
        {
            return BadRequest("Soba s tim brojem već postoji.");
        }

        var room = new Room
        {
            RoomNumber = dto.RoomNumber,
            Capacity = dto.Capacity,
            RoomStatusId = dto.RoomStatusId
        };

        _db.Rooms.Add(room);
        await _db.SaveChangesAsync();
        await _db.Entry(room).Reference(r => r.RoomStatus).LoadAsync();

        var result = MapToDto(room, 0);
        return CreatedAtAction(nameof(GetRoomById), new { id = room.Id }, result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Coordinator")]
    public async Task<IActionResult> UpdateRoom(int id, RoomUpdateDto dto)
    {
        var room = await _db.Rooms.FirstOrDefaultAsync(r => r.Id == id);
        if (room is null)
        {
            return NotFound();
        }

        if (dto.Capacity <= 0)
        {
            return BadRequest("Kapacitet sobe mora biti veći od nule.");
        }

        var statusExists = await _db.RoomStatuses.AnyAsync(s => s.Id == dto.RoomStatusId);
        if (!statusExists)
        {
            return BadRequest("Nepostojeći status sobe.");
        }

        var numberTakenByAnotherRoom = await _db.Rooms.AnyAsync(r => r.Id != id && r.RoomNumber == dto.RoomNumber);
        if (numberTakenByAnotherRoom)
        {
            return BadRequest("Soba s tim brojem već postoji.");
        }

        var currentOccupancy = await GetOccupancyForRoomAsync(id);
        if (dto.Capacity < currentOccupancy)
        {
            return BadRequest($"Kapacitet ne može biti manji od trenutnog broja dodijeljenih korisnika ({currentOccupancy}).");
        }

        room.RoomNumber = dto.RoomNumber;
        room.Capacity = dto.Capacity;
        room.RoomStatusId = dto.RoomStatusId;

        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,Coordinator")]
    public async Task<IActionResult> DeleteRoom(int id)
    {
        var room = await _db.Rooms.FirstOrDefaultAsync(r => r.Id == id);
        if (room is null)
        {
            return NotFound();
        }

        var hasResidents = await _db.Residents.AnyAsync(r => r.RoomId == id);
        if (hasResidents)
        {
            return BadRequest("Soba ima dodijeljene korisnike i ne može se obrisati.");
        }

        _db.Rooms.Remove(room);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    private async Task<Dictionary<int, int>> GetOccupancyByRoomAsync()
    {
        var relevantStatuses = ResidentStatusIds.RacunaSeUPopunjenost;

        return await _db.Residents
            .Where(r => r.RoomId != null && relevantStatuses.Contains(r.ResidentStatusId))
            .GroupBy(r => r.RoomId!.Value)
            .Select(g => new { RoomId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.RoomId, x => x.Count);
    }

    private async Task<int> GetOccupancyForRoomAsync(int roomId)
    {
        var relevantStatuses = ResidentStatusIds.RacunaSeUPopunjenost;

        return await _db.Residents
            .CountAsync(r => r.RoomId == roomId && relevantStatuses.Contains(r.ResidentStatusId));
    }

    private static RoomDto MapToDto(Room room, int occupancy)
    {
        return new RoomDto
        {
            Id = room.Id,
            RoomNumber = room.RoomNumber,
            Capacity = room.Capacity,
            RoomStatusId = room.RoomStatusId,
            RoomStatusName = room.RoomStatus?.Name ?? string.Empty,
            CurrentOccupancy = occupancy,
            FreeCapacity = room.Capacity - occupancy
        };
    }
}
