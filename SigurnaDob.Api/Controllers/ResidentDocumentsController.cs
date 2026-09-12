using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SigurnaDob.Api.Data;
using SigurnaDob.Shared.DTOs.ResidentDocuments;
using SigurnaDob.Shared.Models;

namespace SigurnaDob.Api.Controllers;

[ApiController]
[Route("api/residents/{residentId:int}/documents")]
[Authorize(Roles = "Admin,Coordinator")]
public class ResidentDocumentsController : ControllerBase
{
    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

    private static readonly Dictionary<string, string[]> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        ["application/pdf"] = new[] { ".pdf" },
        ["image/jpeg"] = new[] { ".jpg", ".jpeg" },
        ["image/png"] = new[] { ".png" }
    };

    private readonly SigurnaDobDbContext _db;
    private readonly IWebHostEnvironment _env;

    public ResidentDocumentsController(SigurnaDobDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    [HttpGet]
    public async Task<ActionResult<List<ResidentDocumentDto>>> GetDocuments(int residentId)
    {
        var residentExists = await _db.Residents.AnyAsync(r => r.Id == residentId);
        if (!residentExists)
        {
            return NotFound();
        }

        var documents = await _db.ResidentDocuments
            .Where(d => d.ResidentId == residentId)
            .OrderByDescending(d => d.UploadedAt)
            .ToListAsync();

        return Ok(documents.Select(MapToDto).ToList());
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ResidentDocumentDto>> UploadDocument(int residentId, IFormFile file)
    {
        var resident = await _db.Residents.FirstOrDefaultAsync(r => r.Id == residentId);
        if (resident is null)
        {
            return NotFound();
        }

        if (file is null || file.Length == 0)
        {
            return BadRequest("Datoteka je obavezna.");
        }

        if (file.Length > MaxFileSizeBytes)
        {
            return BadRequest("Datoteka je prevelika. Maksimalna dopuštena veličina je 10 MB.");
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedContentTypes.TryGetValue(file.ContentType, out var allowedExtensions) || !allowedExtensions.Contains(extension))
        {
            return BadRequest("Dopušteni su samo PDF, JPG i PNG dokumenti.");
        }

        var storedFileName = $"{Guid.NewGuid()}{extension}";
        var residentFolder = Path.Combine(_env.ContentRootPath, "Storage", "residents", residentId.ToString());
        Directory.CreateDirectory(residentFolder);
        var filePath = Path.Combine(residentFolder, storedFileName);

        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var document = new ResidentDocument
        {
            ResidentId = residentId,
            OriginalFileName = file.FileName,
            StoredFileName = storedFileName,
            ContentType = file.ContentType,
            FileSizeBytes = file.Length,
            UploadedAt = DateTime.UtcNow
        };

        _db.ResidentDocuments.Add(document);
        await _db.SaveChangesAsync();

        return Created($"/api/residents/{residentId}/documents/{document.Id}", MapToDto(document));
    }

    [HttpGet("{id:int}/download")]
    public async Task<IActionResult> DownloadDocument(int residentId, int id)
    {
        var document = await _db.ResidentDocuments.FirstOrDefaultAsync(d => d.Id == id && d.ResidentId == residentId);
        if (document is null)
        {
            return NotFound();
        }

        var filePath = Path.Combine(_env.ContentRootPath, "Storage", "residents", residentId.ToString(), document.StoredFileName);
        if (!System.IO.File.Exists(filePath))
        {
            return NotFound("Datoteka nije pronađena na disku.");
        }

        return PhysicalFile(filePath, document.ContentType, document.OriginalFileName);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteDocument(int residentId, int id)
    {
        var document = await _db.ResidentDocuments.FirstOrDefaultAsync(d => d.Id == id && d.ResidentId == residentId);
        if (document is null)
        {
            return NotFound();
        }

        var filePath = Path.Combine(_env.ContentRootPath, "Storage", "residents", residentId.ToString(), document.StoredFileName);

        try
        {
            System.IO.File.Delete(filePath);
        }
        catch (IOException)
        {
            return StatusCode(500, "Datoteka je trenutno u upotrebi i ne može se obrisati. Pokušaj ponovno.");
        }
        catch (UnauthorizedAccessException)
        {
            return StatusCode(500, "Nema dozvole za brisanje datoteke s diska.");
        }

        _db.ResidentDocuments.Remove(document);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    private static ResidentDocumentDto MapToDto(ResidentDocument document)
    {
        return new ResidentDocumentDto
        {
            Id = document.Id,
            ResidentId = document.ResidentId,
            OriginalFileName = document.OriginalFileName,
            ContentType = document.ContentType,
            FileSizeBytes = document.FileSizeBytes,
            UploadedAt = document.UploadedAt
        };
    }
}
