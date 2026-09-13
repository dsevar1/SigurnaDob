using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SigurnaDob.Api.Data;
using SigurnaDob.Shared.DTOs.FamilyContacts;
using SigurnaDob.Shared.Models;

namespace SigurnaDob.Api.Controllers;

[ApiController]
[Route("api/residents/{residentId:int}/family-contacts")]
[Authorize(Roles = "Admin,Coordinator")]
public class FamilyContactsController : ControllerBase
{
    private readonly SigurnaDobDbContext _db;

    public FamilyContactsController(SigurnaDobDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<List<FamilyContactDto>>> GetFamilyContacts(int residentId)
    {
        var residentExists = await _db.Residents.AnyAsync(r => r.Id == residentId);
        if (!residentExists)
        {
            return NotFound();
        }

        var contacts = await _db.FamilyContacts
            .Include(fc => fc.Resident)
            .Where(fc => fc.ResidentId == residentId)
            .OrderBy(fc => fc.FullName)
            .ToListAsync();

        return Ok(contacts.Select(MapToDto).ToList());
    }

    // Apsolutna ruta (bez residentId prefiksa) - namjerno u istom kontroleru jer je logika
    // identična, samo bez filtera po jednom residentu. Koristi Users stranica (admin bira IZMEĐU
    // svih obiteljskih kontakata u sustavu, ne samo jednog residenta).
    [HttpGet("/api/family-contacts")]
    public async Task<ActionResult<List<FamilyContactDto>>> GetAllFamilyContacts()
    {
        var contacts = await _db.FamilyContacts
            .Include(fc => fc.Resident)
            .OrderBy(fc => fc.Resident!.FullName)
            .ThenBy(fc => fc.FullName)
            .ToListAsync();

        return Ok(contacts.Select(MapToDto).ToList());
    }

    [HttpPost]
    public async Task<ActionResult<FamilyContactDto>> CreateFamilyContact(int residentId, FamilyContactCreateDto dto)
    {
        var residentExists = await _db.Residents.AnyAsync(r => r.Id == residentId);
        if (!residentExists)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(dto.FullName))
        {
            return BadRequest("Ime i prezime obiteljskog kontakta su obavezni.");
        }

        var contact = new FamilyContact
        {
            ResidentId = residentId,
            FullName = dto.FullName,
            Relationship = dto.Relationship,
            Phone = dto.Phone,
            Email = dto.Email
        };

        _db.FamilyContacts.Add(contact);
        await _db.SaveChangesAsync();
        await _db.Entry(contact).Reference(fc => fc.Resident).LoadAsync();

        return Created($"/api/residents/{residentId}/family-contacts/{contact.Id}", MapToDto(contact));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateFamilyContact(int residentId, int id, FamilyContactUpdateDto dto)
    {
        var contact = await _db.FamilyContacts.FirstOrDefaultAsync(fc => fc.Id == id && fc.ResidentId == residentId);
        if (contact is null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(dto.FullName))
        {
            return BadRequest("Ime i prezime obiteljskog kontakta su obavezni.");
        }

        contact.FullName = dto.FullName;
        contact.Relationship = dto.Relationship;
        contact.Phone = dto.Phone;
        contact.Email = dto.Email;

        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteFamilyContact(int residentId, int id)
    {
        var contact = await _db.FamilyContacts.FirstOrDefaultAsync(fc => fc.Id == id && fc.ResidentId == residentId);
        if (contact is null)
        {
            return NotFound();
        }

        var hasVisitRequests = await _db.VisitRequests.AnyAsync(vr => vr.FamilyContactId == id);
        if (hasVisitRequests)
        {
            return BadRequest("Obiteljski kontakt ima postojeće zahtjeve za posjet i ne može se obrisati.");
        }

        _db.FamilyContacts.Remove(contact);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    private static FamilyContactDto MapToDto(FamilyContact contact)
    {
        return new FamilyContactDto
        {
            Id = contact.Id,
            ResidentId = contact.ResidentId,
            ResidentFullName = contact.Resident?.FullName ?? string.Empty,
            FullName = contact.FullName,
            Relationship = contact.Relationship,
            Phone = contact.Phone,
            Email = contact.Email
        };
    }
}
