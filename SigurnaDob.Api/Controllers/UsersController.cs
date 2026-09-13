using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SigurnaDob.Api.Data;
using SigurnaDob.Shared.DTOs.Users;
using SigurnaDob.Shared.Models;

namespace SigurnaDob.Api.Controllers;

// Class-level [Authorize(Roles = "Admin")] je sigurno ovdje - upravljanje korisničkim računima je
// ISKLJUČIVO Admin funkcionalnost po specifikaciji, nema akcije za drugu ulogu (za razliku od
// CareTasks/VisitRequests), pa nema AND-kombinacija rizika.
[ApiController]
[Route("api/users")]
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly SigurnaDobDbContext _db;
    private readonly IPasswordHasher<AppUser> _passwordHasher;

    public UsersController(SigurnaDobDbContext db, IPasswordHasher<AppUser> passwordHasher)
    {
        _db = db;
        _passwordHasher = passwordHasher;
    }

    [HttpGet]
    public async Task<ActionResult<List<AppUserDto>>> GetUsers()
    {
        var users = await LoadUsersQuery().OrderBy(u => u.Username).ToListAsync();
        return Ok(users.Select(MapToDto).ToList());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AppUserDto>> GetUserById(int id)
    {
        var user = await LoadUsersQuery().FirstOrDefaultAsync(u => u.Id == id);
        if (user is null)
        {
            return NotFound();
        }

        return Ok(MapToDto(user));
    }

    [HttpPost]
    public async Task<ActionResult<AppUserDto>> CreateUser(AppUserCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Username))
        {
            return BadRequest("Korisničko ime je obavezno.");
        }

        var usernameTaken = await _db.AppUsers.AnyAsync(u => u.Username == dto.Username);
        if (usernameTaken)
        {
            return BadRequest("Korisničko ime je već zauzeto.");
        }

        if (string.IsNullOrWhiteSpace(dto.Password))
        {
            return BadRequest("Lozinka je obavezna.");
        }

        var validationError = await ValidateRolesAndLinksAsync(dto.RoleIds, dto.StaffId, dto.FamilyContactId, existingUserId: null);
        if (validationError is not null)
        {
            return validationError;
        }

        var user = new AppUser
        {
            Username = dto.Username,
            IsActive = true,
            StaffId = dto.StaffId,
            FamilyContactId = dto.FamilyContactId,
            CreatedAt = DateTime.UtcNow
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);

        _db.AppUsers.Add(user);
        await _db.SaveChangesAsync();

        await SetRolesAsync(user.Id, dto.RoleIds);
        await _db.SaveChangesAsync();

        var created = await LoadUsersQuery().FirstAsync(u => u.Id == user.Id);
        return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, MapToDto(created));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateUser(int id, AppUserUpdateDto dto)
    {
        var user = await _db.AppUsers.FirstOrDefaultAsync(u => u.Id == id);
        if (user is null)
        {
            return NotFound();
        }

        var validationError = await ValidateRolesAndLinksAsync(dto.RoleIds, dto.StaffId, dto.FamilyContactId, existingUserId: id);
        if (validationError is not null)
        {
            return validationError;
        }

        user.StaffId = dto.StaffId;
        user.FamilyContactId = dto.FamilyContactId;
        await SetRolesAsync(id, dto.RoleIds);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("{id:int}/activate")]
    public async Task<IActionResult> ActivateUser(int id)
    {
        var user = await _db.AppUsers.FirstOrDefaultAsync(u => u.Id == id);
        if (user is null)
        {
            return NotFound();
        }

        user.IsActive = true;
        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("{id:int}/deactivate")]
    public async Task<IActionResult> DeactivateUser(int id)
    {
        if (GetCurrentUserId() == id)
        {
            return BadRequest("Ne možeš deaktivirati vlastiti račun.");
        }

        var user = await _db.AppUsers.FirstOrDefaultAsync(u => u.Id == id);
        if (user is null)
        {
            return NotFound();
        }

        user.IsActive = false;
        await _db.SaveChangesAsync();

        return NoContent();
    }

    private async Task<ActionResult?> ValidateRolesAndLinksAsync(
        List<int> roleIds, int? staffId, int? familyContactId, int? existingUserId)
    {
        if (roleIds is null || roleIds.Count == 0)
        {
            return BadRequest("Barem jedna uloga je obavezna.");
        }

        var distinctRoleIds = roleIds.Distinct().ToList();
        var validRoleCount = await _db.AppRoles.CountAsync(r => distinctRoleIds.Contains(r.Id));
        if (validRoleCount != distinctRoleIds.Count)
        {
            return BadRequest("Jedna ili više odabranih uloga ne postoji.");
        }

        if (staffId is not null && familyContactId is not null)
        {
            return BadRequest("Račun se može povezati najviše s jednim profilom - ili djelatnikom, ili obiteljskim kontaktom, ne oboje.");
        }

        if (staffId is not null)
        {
            var staffExists = await _db.Staff.AnyAsync(s => s.Id == staffId);
            if (!staffExists)
            {
                return NotFound("Djelatnik ne postoji.");
            }

            var staffAlreadyLinked = await _db.AppUsers
                .AnyAsync(u => u.StaffId == staffId && u.Id != (existingUserId ?? -1));
            if (staffAlreadyLinked)
            {
                return BadRequest("Odabrani djelatnik je već povezan s drugim računom.");
            }
        }

        if (familyContactId is not null)
        {
            var contactExists = await _db.FamilyContacts.AnyAsync(fc => fc.Id == familyContactId);
            if (!contactExists)
            {
                return NotFound("Obiteljski kontakt ne postoji.");
            }

            var contactAlreadyLinked = await _db.AppUsers
                .AnyAsync(u => u.FamilyContactId == familyContactId && u.Id != (existingUserId ?? -1));
            if (contactAlreadyLinked)
            {
                return BadRequest("Odabrani obiteljski kontakt je već povezan s drugim računom.");
            }
        }

        return null;
    }

    private async Task SetRolesAsync(int userId, List<int> roleIds)
    {
        var existingRoles = await _db.AppUserRoles.Where(ur => ur.AppUserId == userId).ToListAsync();
        _db.AppUserRoles.RemoveRange(existingRoles);

        foreach (var roleId in roleIds.Distinct())
        {
            _db.AppUserRoles.Add(new AppUserRole { AppUserId = userId, AppRoleId = roleId });
        }
    }

    private int? GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (claim is null || !int.TryParse(claim.Value, out var userId))
        {
            return null;
        }

        return userId;
    }

    private IQueryable<AppUser> LoadUsersQuery()
    {
        return _db.AppUsers
            .Include(u => u.AppUserRoles).ThenInclude(ur => ur.AppRole)
            .Include(u => u.Staff)
            .Include(u => u.FamilyContact);
    }

    private static AppUserDto MapToDto(AppUser user)
    {
        return new AppUserDto
        {
            Id = user.Id,
            Username = user.Username,
            IsActive = user.IsActive,
            Roles = user.AppUserRoles.Select(ur => ur.AppRole!.Name).OrderBy(n => n).ToList(),
            StaffId = user.StaffId,
            StaffFullName = user.Staff?.FullName,
            FamilyContactId = user.FamilyContactId,
            FamilyContactFullName = user.FamilyContact?.FullName
        };
    }
}
