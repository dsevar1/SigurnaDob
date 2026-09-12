using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SigurnaDob.Api.Data;
using SigurnaDob.Shared.Constants;
using SigurnaDob.Shared.DTOs.CareTasks;
using SigurnaDob.Shared.Models;

namespace SigurnaDob.Api.Controllers;

// NAMJERNO bez [Authorize] na razini klase: ASP.NET Core kombinira class-level i method-level
// [Authorize] atribute AND logikom (moraš zadovoljiti SVE), ne override. Kad bi klasa nosila
// "Admin,Coordinator" a /mine, /start, /complete "Caregiver", nijedan korisnik ne bi mogao proći.
// Zato svaka akcija ima svoj eksplicitan [Authorize(Roles = ...)].
[ApiController]
[Route("api/caretasks")]
public class CareTasksController : ControllerBase
{
    private readonly SigurnaDobDbContext _db;

    public CareTasksController(SigurnaDobDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Coordinator")]
    public async Task<ActionResult<List<CareTaskDto>>> GetCareTasks(
        [FromQuery] string? search,
        [FromQuery] int? careTaskStatusId,
        [FromQuery] int? careTaskTypeId,
        [FromQuery] int? assignedStaffId,
        [FromQuery] DateTime? dueDateFrom,
        [FromQuery] DateTime? dueDateTo,
        [FromQuery] string? sortBy,
        [FromQuery] bool sortDescending = false)
    {
        var query = _db.CareTasks
            .Include(t => t.Resident)
            .Include(t => t.CareTaskType)
            .Include(t => t.CareTaskStatus)
            .Include(t => t.AssignedStaff)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(t => t.Resident!.FullName.Contains(search));
        }

        if (careTaskStatusId is not null)
        {
            query = query.Where(t => t.CareTaskStatusId == careTaskStatusId);
        }

        if (careTaskTypeId is not null)
        {
            query = query.Where(t => t.CareTaskTypeId == careTaskTypeId);
        }

        if (assignedStaffId is not null)
        {
            query = query.Where(t => t.AssignedStaffId == assignedStaffId);
        }

        if (dueDateFrom is not null)
        {
            query = query.Where(t => t.DueDate >= dueDateFrom);
        }

        if (dueDateTo is not null)
        {
            query = query.Where(t => t.DueDate <= dueDateTo);
        }

        query = sortBy?.ToLowerInvariant() switch
        {
            "status" => sortDescending ? query.OrderByDescending(t => t.CareTaskStatus!.Name) : query.OrderBy(t => t.CareTaskStatus!.Name),
            "type" => sortDescending ? query.OrderByDescending(t => t.CareTaskType!.Name) : query.OrderBy(t => t.CareTaskType!.Name),
            "resident" => sortDescending ? query.OrderByDescending(t => t.Resident!.FullName) : query.OrderBy(t => t.Resident!.FullName),
            "staff" => sortDescending ? query.OrderByDescending(t => t.AssignedStaff!.FullName) : query.OrderBy(t => t.AssignedStaff!.FullName),
            _ => sortDescending ? query.OrderByDescending(t => t.DueDate) : query.OrderBy(t => t.DueDate)
        };

        var tasks = await query.ToListAsync();
        return Ok(tasks.Select(MapToDto).ToList());
    }

    [HttpGet("mine")]
    [Authorize(Roles = "Caregiver")]
    public async Task<ActionResult<List<CareTaskDto>>> GetMyCareTasks()
    {
        var staffId = GetStaffIdFromClaims();
        if (staffId is null)
        {
            return Forbid();
        }

        var tasks = await _db.CareTasks
            .Include(t => t.Resident)
            .Include(t => t.CareTaskType)
            .Include(t => t.CareTaskStatus)
            .Include(t => t.AssignedStaff)
            .Where(t => t.AssignedStaffId == staffId)
            .OrderBy(t => t.DueDate)
            .ToListAsync();

        return Ok(tasks.Select(MapToDto).ToList());
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin,Coordinator")]
    public async Task<ActionResult<CareTaskDto>> GetCareTaskById(int id)
    {
        var task = await _db.CareTasks
            .Include(t => t.Resident)
            .Include(t => t.CareTaskType)
            .Include(t => t.CareTaskStatus)
            .Include(t => t.AssignedStaff)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (task is null)
        {
            return NotFound();
        }

        return Ok(MapToDto(task));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Coordinator")]
    public async Task<ActionResult<CareTaskDto>> CreateCareTask(CareTaskCreateDto dto)
    {
        var validationError = await ValidateAsync(dto.ResidentId, dto.CareTaskTypeId, dto.CareTaskStatusId, dto.DueDate, dto.AssignedStaffId);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        var task = new CareTask
        {
            ResidentId = dto.ResidentId,
            CareTaskTypeId = dto.CareTaskTypeId,
            CareTaskStatusId = dto.CareTaskStatusId,
            DueDate = dto.DueDate,
            AssignedStaffId = dto.AssignedStaffId,
            CreatedAt = DateTime.UtcNow
        };

        _db.CareTasks.Add(task);
        await _db.SaveChangesAsync();

        await _db.Entry(task).Reference(t => t.Resident).LoadAsync();
        await _db.Entry(task).Reference(t => t.CareTaskType).LoadAsync();
        await _db.Entry(task).Reference(t => t.CareTaskStatus).LoadAsync();
        if (task.AssignedStaffId is not null)
        {
            await _db.Entry(task).Reference(t => t.AssignedStaff).LoadAsync();
        }

        return CreatedAtAction(nameof(GetCareTaskById), new { id = task.Id }, MapToDto(task));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Coordinator")]
    public async Task<IActionResult> UpdateCareTask(int id, CareTaskUpdateDto dto)
    {
        var task = await _db.CareTasks.FirstOrDefaultAsync(t => t.Id == id);
        if (task is null)
        {
            return NotFound();
        }

        var validationError = await ValidateAsync(dto.ResidentId, dto.CareTaskTypeId, dto.CareTaskStatusId, dto.DueDate, dto.AssignedStaffId);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        task.ResidentId = dto.ResidentId;
        task.CareTaskTypeId = dto.CareTaskTypeId;
        task.CareTaskStatusId = dto.CareTaskStatusId;
        task.DueDate = dto.DueDate;
        task.AssignedStaffId = dto.AssignedStaffId;

        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("{id:int}/start")]
    [Authorize(Roles = "Caregiver")]
    public async Task<ActionResult<CareTaskDto>> StartCareTask(int id)
    {
        var staffId = GetStaffIdFromClaims();
        if (staffId is null)
        {
            return Forbid();
        }

        var task = await _db.CareTasks
            .Include(t => t.Resident)
            .Include(t => t.CareTaskType)
            .Include(t => t.AssignedStaff)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (task is null)
        {
            return NotFound();
        }

        if (task.AssignedStaffId != staffId)
        {
            return Forbid();
        }

        if (task.CareTaskStatusId != CareTaskStatusIds.Dodijeljeno)
        {
            return BadRequest("Zadatak nije u statusu Dodijeljeno.");
        }

        task.CareTaskStatusId = CareTaskStatusIds.UTijeku;
        task.CareTaskStatus = await _db.CareTaskStatuses.FindAsync(task.CareTaskStatusId);

        await _db.SaveChangesAsync();

        return Ok(MapToDto(task));
    }

    [HttpPost("{id:int}/complete")]
    [Authorize(Roles = "Caregiver")]
    public async Task<ActionResult<CareTaskDto>> CompleteCareTask(int id, CareTaskCompleteDto dto)
    {
        var staffId = GetStaffIdFromClaims();
        if (staffId is null)
        {
            return Forbid();
        }

        var task = await _db.CareTasks
            .Include(t => t.Resident)
            .Include(t => t.CareTaskType)
            .Include(t => t.AssignedStaff)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (task is null)
        {
            return NotFound();
        }

        if (task.AssignedStaffId != staffId)
        {
            return Forbid();
        }

        if (task.CareTaskStatusId != CareTaskStatusIds.UTijeku)
        {
            return BadRequest("Zadatak nije u statusu U tijeku.");
        }

        if (string.IsNullOrWhiteSpace(dto.CompletionNote))
        {
            return BadRequest("Bilješka je obavezna.");
        }

        if (dto.CompletedAt > DateTime.UtcNow)
        {
            return BadRequest("Vrijeme završetka ne smije biti u budućnosti.");
        }

        if (dto.CompletedAt < task.CreatedAt)
        {
            return BadRequest("Vrijeme završetka ne smije biti prije vremena kreiranja zadatka.");
        }

        task.CareTaskStatusId = CareTaskStatusIds.Izvrseno;
        task.CareTaskStatus = await _db.CareTaskStatuses.FindAsync(task.CareTaskStatusId);
        task.CompletedAt = dto.CompletedAt;
        task.CompletionNote = dto.CompletionNote;

        await _db.SaveChangesAsync();

        return Ok(MapToDto(task));
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

    private async Task<string?> ValidateAsync(int residentId, int careTaskTypeId, int careTaskStatusId, DateTime dueDate, int? assignedStaffId)
    {
        var residentExists = await _db.Residents.AnyAsync(r => r.Id == residentId);
        if (!residentExists)
        {
            return "Nepostojeći korisnik doma.";
        }

        var typeExists = await _db.CareTaskTypes.AnyAsync(t => t.Id == careTaskTypeId);
        if (!typeExists)
        {
            return "Nepostojeći tip zadatka.";
        }

        var statusExists = await _db.CareTaskStatuses.AnyAsync(s => s.Id == careTaskStatusId);
        if (!statusExists)
        {
            return "Nepostojeći status zadatka.";
        }

        if (dueDate == default)
        {
            return "Rok je obavezan.";
        }

        if (assignedStaffId is not null)
        {
            var staff = await _db.Staff.FirstOrDefaultAsync(s => s.Id == assignedStaffId);
            if (staff is null)
            {
                return "Nepostojeći djelatnik.";
            }

            if (!staff.IsActive)
            {
                return "Djelatnik nije aktivan.";
            }
        }

        return null;
    }

    private static CareTaskDto MapToDto(CareTask task)
    {
        return new CareTaskDto
        {
            Id = task.Id,
            ResidentId = task.ResidentId,
            ResidentFullName = task.Resident?.FullName ?? string.Empty,
            CareTaskTypeId = task.CareTaskTypeId,
            CareTaskTypeName = task.CareTaskType?.Name ?? string.Empty,
            CareTaskStatusId = task.CareTaskStatusId,
            CareTaskStatusName = task.CareTaskStatus?.Name ?? string.Empty,
            AssignedStaffId = task.AssignedStaffId,
            AssignedStaffFullName = task.AssignedStaff?.FullName,
            DueDate = task.DueDate,
            CreatedAt = task.CreatedAt,
            CompletedAt = task.CompletedAt,
            CompletionNote = task.CompletionNote
        };
    }
}
