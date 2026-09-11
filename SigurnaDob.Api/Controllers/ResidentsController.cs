using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SigurnaDob.Api.Data;
using SigurnaDob.Api.Services;
using SigurnaDob.Shared.Constants;
using SigurnaDob.Shared.DTOs.Residents;
using SigurnaDob.Shared.Models;

namespace SigurnaDob.Api.Controllers;

[ApiController]
[Route("api/residents")]
[Authorize(Roles = "Admin,Coordinator")]
public class ResidentsController : ControllerBase
{
    private readonly SigurnaDobDbContext _db;
    private readonly RoomOccupancyService _occupancy;

    public ResidentsController(SigurnaDobDbContext db, RoomOccupancyService occupancy)
    {
        _db = db;
        _occupancy = occupancy;
    }

    [HttpGet]
    public async Task<ActionResult<List<ResidentDto>>> GetResidents(
        [FromQuery] string? search,
        [FromQuery] int? residentStatusId,
        [FromQuery] int? roomId,
        [FromQuery] string? sortBy,
        [FromQuery] bool sortDescending = false)
    {
        var query = _db.Residents
            .Include(r => r.ResidentStatus)
            .Include(r => r.Room)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(r => r.FullName.Contains(search));
        }

        if (residentStatusId is not null)
        {
            query = query.Where(r => r.ResidentStatusId == residentStatusId);
        }

        if (roomId is not null)
        {
            query = query.Where(r => r.RoomId == roomId);
        }

        query = sortBy?.ToLowerInvariant() switch
        {
            "status" => sortDescending ? query.OrderByDescending(r => r.ResidentStatus!.Name) : query.OrderBy(r => r.ResidentStatus!.Name),
            "room" => sortDescending ? query.OrderByDescending(r => r.Room!.RoomNumber) : query.OrderBy(r => r.Room!.RoomNumber),
            _ => sortDescending ? query.OrderByDescending(r => r.FullName) : query.OrderBy(r => r.FullName)
        };

        var residents = await query.ToListAsync();
        return Ok(residents.Select(MapToDto).ToList());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ResidentDto>> GetResidentById(int id)
    {
        var resident = await _db.Residents
            .Include(r => r.ResidentStatus)
            .Include(r => r.Room)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (resident is null)
        {
            return NotFound();
        }

        return Ok(MapToDto(resident));
    }

    [HttpPost]
    public async Task<ActionResult<ResidentDto>> CreateResident(ResidentCreateDto dto)
    {
        var validationError = await ValidateAsync(dto.FullName, dto.ResidentStatusId, dto.RoomId, excludeResidentId: null);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        var resident = new Resident
        {
            FullName = dto.FullName,
            DateOfBirth = dto.DateOfBirth,
            ResidentStatusId = dto.ResidentStatusId,
            RoomId = dto.RoomId,
            AdmissionDate = dto.AdmissionDate,
            CreatedAt = DateTime.UtcNow
        };

        _db.Residents.Add(resident);
        await _db.SaveChangesAsync();

        await _db.Entry(resident).Reference(r => r.ResidentStatus).LoadAsync();
        if (resident.RoomId is not null)
        {
            await _db.Entry(resident).Reference(r => r.Room).LoadAsync();
        }

        return CreatedAtAction(nameof(GetResidentById), new { id = resident.Id }, MapToDto(resident));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateResident(int id, ResidentUpdateDto dto)
    {
        var resident = await _db.Residents.FirstOrDefaultAsync(r => r.Id == id);
        if (resident is null)
        {
            return NotFound();
        }

        var validationError = await ValidateAsync(dto.FullName, dto.ResidentStatusId, dto.RoomId, excludeResidentId: id);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        resident.FullName = dto.FullName;
        resident.DateOfBirth = dto.DateOfBirth;
        resident.ResidentStatusId = dto.ResidentStatusId;
        resident.RoomId = dto.RoomId;
        resident.AdmissionDate = dto.AdmissionDate;

        await _db.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Zajednička validacija za create i update. Redoslijed provjera:
    /// ime → status postoji → Aktivan zahtijeva sobu → Premješten/Arhiviran ne smije imati sobu →
    /// soba postoji → soba nije Održavanje/Izvan uporabe → kapacitet sobe nije prekoračen
    /// (samo kad novi status residenta ulazi u izračun popunjenosti).
    /// </summary>
    private async Task<string?> ValidateAsync(string fullName, int residentStatusId, int? roomId, int? excludeResidentId)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            return "Ime i prezime su obavezni.";
        }

        var statusExists = await _db.ResidentStatuses.AnyAsync(s => s.Id == residentStatusId);
        if (!statusExists)
        {
            return "Nepostojeći status korisnika doma.";
        }

        if (residentStatusId == ResidentStatusIds.Aktivan && roomId is null)
        {
            return "Korisnik sa statusom Aktivan mora imati dodijeljenu sobu.";
        }

        if ((residentStatusId == ResidentStatusIds.PremjestenIzDoma || residentStatusId == ResidentStatusIds.Arhiviran) && roomId is not null)
        {
            return "Korisnik sa statusom Premješten iz doma ili Arhiviran ne smije imati dodijeljenu sobu.";
        }

        if (roomId is not null)
        {
            var room = await _db.Rooms.FirstOrDefaultAsync(r => r.Id == roomId);
            if (room is null)
            {
                return "Nepostojeća soba.";
            }

            if (RoomStatusIds.ZabranjujuDodjelu.Contains(room.RoomStatusId))
            {
                return "Korisnik se ne može smjestiti u sobu koja je u statusu Održavanje ili Izvan uporabe.";
            }

            if (ResidentStatusIds.RacunaSeUPopunjenost.Contains(residentStatusId))
            {
                var currentOccupancy = await _occupancy.GetOccupancyAsync(roomId.Value, excludeResidentId);
                if (currentOccupancy + 1 > room.Capacity)
                {
                    return $"Soba {room.RoomNumber} je popunjena (kapacitet {room.Capacity}).";
                }
            }
        }

        return null;
    }

    private static ResidentDto MapToDto(Resident resident)
    {
        return new ResidentDto
        {
            Id = resident.Id,
            FullName = resident.FullName,
            DateOfBirth = resident.DateOfBirth,
            ResidentStatusId = resident.ResidentStatusId,
            ResidentStatusName = resident.ResidentStatus?.Name ?? string.Empty,
            RoomId = resident.RoomId,
            RoomNumber = resident.Room?.RoomNumber,
            AdmissionDate = resident.AdmissionDate
        };
    }
}
