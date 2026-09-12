using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SigurnaDob.Api.Data;
using SigurnaDob.Shared.Constants;
using SigurnaDob.Shared.DTOs.VisitRequests;
using SigurnaDob.Shared.Models;

namespace SigurnaDob.Api.Controllers;

// NAMJERNO bez [Authorize] na razini klase - isti razlog kao CareTasksController: ASP.NET Core
// kombinira class-level i method-level [Authorize] AND logikom, što bi blokiralo miješane uloge
// (Admin/Coordinator na CRUD/odluke, FamilyMember na /mine).
[ApiController]
[Route("api/visitrequests")]
public class VisitRequestsController : ControllerBase
{
    private readonly SigurnaDobDbContext _db;

    public VisitRequestsController(SigurnaDobDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Coordinator")]
    public async Task<ActionResult<List<VisitRequestDto>>> GetVisitRequests(
        [FromQuery] int? visitRequestStatusId,
        [FromQuery] string? sortBy,
        [FromQuery] bool sortDescending = false)
    {
        var query = _db.VisitRequests
            .Include(v => v.Resident)
            .Include(v => v.FamilyContact)
            .Include(v => v.VisitRequestStatus)
            .Include(v => v.DecidedByStaff)
            .AsQueryable();

        if (visitRequestStatusId is not null)
        {
            query = query.Where(v => v.VisitRequestStatusId == visitRequestStatusId);
        }

        query = sortBy?.ToLowerInvariant() switch
        {
            "resident" => sortDescending ? query.OrderByDescending(v => v.Resident!.FullName) : query.OrderBy(v => v.Resident!.FullName),
            "familycontact" => sortDescending ? query.OrderByDescending(v => v.FamilyContact!.FullName) : query.OrderBy(v => v.FamilyContact!.FullName),
            "status" => sortDescending ? query.OrderByDescending(v => v.VisitRequestStatus!.Name) : query.OrderBy(v => v.VisitRequestStatus!.Name),
            _ => sortDescending ? query.OrderByDescending(v => v.RequestedAt) : query.OrderBy(v => v.RequestedAt)
        };

        var requests = await query.ToListAsync();
        return Ok(requests.Select(MapToDto).ToList());
    }

    [HttpGet("mine")]
    [Authorize(Roles = "FamilyMember")]
    public async Task<ActionResult<List<VisitRequestDto>>> GetMyVisitRequests()
    {
        var familyContactId = GetFamilyContactIdFromClaims();
        if (familyContactId is null)
        {
            return Forbid();
        }

        var requests = await _db.VisitRequests
            .Include(v => v.Resident)
            .Include(v => v.FamilyContact)
            .Include(v => v.VisitRequestStatus)
            .Include(v => v.DecidedByStaff)
            .Where(v => v.FamilyContactId == familyContactId)
            .OrderBy(v => v.RequestedAt)
            .ToListAsync();

        return Ok(requests.Select(MapToDto).ToList());
    }

    [HttpPost("mine")]
    [Authorize(Roles = "FamilyMember")]
    public async Task<ActionResult<VisitRequestDto>> CreateMyVisitRequest(VisitRequestCreateDto dto)
    {
        var familyContactId = GetFamilyContactIdFromClaims();
        if (familyContactId is null)
        {
            return Forbid();
        }

        var familyContact = await _db.FamilyContacts.FirstOrDefaultAsync(fc => fc.Id == familyContactId);
        if (familyContact is null)
        {
            return NotFound();
        }

        if (dto.RequestedAt == default)
        {
            return BadRequest("Traženi termin posjeta je obavezan.");
        }

        var visitRequest = new VisitRequest
        {
            ResidentId = familyContact.ResidentId,
            FamilyContactId = familyContact.Id,
            VisitRequestStatusId = VisitRequestStatusIds.Zaprimljeno,
            RequestedAt = dto.RequestedAt,
            CreatedAt = DateTime.UtcNow
        };

        _db.VisitRequests.Add(visitRequest);
        await _db.SaveChangesAsync();

        await _db.Entry(visitRequest).Reference(v => v.Resident).LoadAsync();
        await _db.Entry(visitRequest).Reference(v => v.FamilyContact).LoadAsync();
        await _db.Entry(visitRequest).Reference(v => v.VisitRequestStatus).LoadAsync();

        return Created($"/api/visitrequests/{visitRequest.Id}", MapToDto(visitRequest));
    }

    [HttpPost("{id:int}/approve")]
    [Authorize(Roles = "Admin,Coordinator")]
    public async Task<ActionResult<VisitRequestDto>> ApproveVisitRequest(int id, VisitRequestDecisionDto dto)
    {
        var visitRequest = await LoadVisitRequestAsync(id);
        if (visitRequest is null)
        {
            return NotFound();
        }

        if (visitRequest.VisitRequestStatusId != VisitRequestStatusIds.Zaprimljeno)
        {
            return BadRequest("Zahtjev nije u statusu Zaprimljeno.");
        }

        if (visitRequest.RequestedAt < DateTime.UtcNow)
        {
            return BadRequest("Ne može se odobriti posjet za datum u prošlosti.");
        }

        visitRequest.VisitRequestStatusId = VisitRequestStatusIds.Odobreno;
        visitRequest.VisitRequestStatus = await _db.VisitRequestStatuses.FindAsync(visitRequest.VisitRequestStatusId);
        visitRequest.DecisionNote = dto.DecisionNote;
        visitRequest.DecidedByStaffId = GetStaffIdFromClaims();
        if (visitRequest.DecidedByStaffId is not null)
        {
            await _db.Entry(visitRequest).Reference(v => v.DecidedByStaff).LoadAsync();
        }

        await _db.SaveChangesAsync();

        return Ok(MapToDto(visitRequest));
    }

    [HttpPost("{id:int}/reject")]
    [Authorize(Roles = "Admin,Coordinator")]
    public async Task<ActionResult<VisitRequestDto>> RejectVisitRequest(int id, VisitRequestDecisionDto dto)
    {
        var visitRequest = await LoadVisitRequestAsync(id);
        if (visitRequest is null)
        {
            return NotFound();
        }

        if (visitRequest.VisitRequestStatusId != VisitRequestStatusIds.Zaprimljeno)
        {
            return BadRequest("Zahtjev nije u statusu Zaprimljeno.");
        }

        visitRequest.VisitRequestStatusId = VisitRequestStatusIds.Odbijeno;
        visitRequest.VisitRequestStatus = await _db.VisitRequestStatuses.FindAsync(visitRequest.VisitRequestStatusId);
        visitRequest.DecisionNote = dto.DecisionNote;
        visitRequest.DecidedByStaffId = GetStaffIdFromClaims();
        if (visitRequest.DecidedByStaffId is not null)
        {
            await _db.Entry(visitRequest).Reference(v => v.DecidedByStaff).LoadAsync();
        }

        await _db.SaveChangesAsync();

        return Ok(MapToDto(visitRequest));
    }

    [HttpPost("{id:int}/mark-held")]
    [Authorize(Roles = "Admin,Coordinator")]
    public async Task<ActionResult<VisitRequestDto>> MarkVisitRequestHeld(int id, VisitRequestDecisionDto dto)
    {
        return await SetOutcomeAsync(id, VisitRequestStatusIds.Odrzano, dto);
    }

    [HttpPost("{id:int}/mark-cancelled")]
    [Authorize(Roles = "Admin,Coordinator")]
    public async Task<ActionResult<VisitRequestDto>> MarkVisitRequestCancelled(int id, VisitRequestDecisionDto dto)
    {
        return await SetOutcomeAsync(id, VisitRequestStatusIds.Otkazano, dto);
    }

    private async Task<ActionResult<VisitRequestDto>> SetOutcomeAsync(int id, int newStatusId, VisitRequestDecisionDto dto)
    {
        var visitRequest = await LoadVisitRequestAsync(id);
        if (visitRequest is null)
        {
            return NotFound();
        }

        if (visitRequest.VisitRequestStatusId != VisitRequestStatusIds.Odobreno)
        {
            return BadRequest("Zahtjev nije u statusu Odobreno.");
        }

        visitRequest.VisitRequestStatusId = newStatusId;
        visitRequest.VisitRequestStatus = await _db.VisitRequestStatuses.FindAsync(newStatusId);
        if (dto.DecisionNote is not null)
        {
            visitRequest.DecisionNote = dto.DecisionNote;
        }

        await _db.SaveChangesAsync();

        return Ok(MapToDto(visitRequest));
    }

    private async Task<VisitRequest?> LoadVisitRequestAsync(int id)
    {
        return await _db.VisitRequests
            .Include(v => v.Resident)
            .Include(v => v.FamilyContact)
            .Include(v => v.VisitRequestStatus)
            .Include(v => v.DecidedByStaff)
            .FirstOrDefaultAsync(v => v.Id == id);
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

    private static VisitRequestDto MapToDto(VisitRequest v)
    {
        return new VisitRequestDto
        {
            Id = v.Id,
            ResidentId = v.ResidentId,
            ResidentFullName = v.Resident?.FullName ?? string.Empty,
            FamilyContactId = v.FamilyContactId,
            FamilyContactFullName = v.FamilyContact?.FullName ?? string.Empty,
            VisitRequestStatusId = v.VisitRequestStatusId,
            VisitRequestStatusName = v.VisitRequestStatus?.Name ?? string.Empty,
            RequestedAt = v.RequestedAt,
            DecidedByStaffId = v.DecidedByStaffId,
            DecidedByStaffFullName = v.DecidedByStaff?.FullName,
            DecisionNote = v.DecisionNote,
            CreatedAt = v.CreatedAt
        };
    }
}
