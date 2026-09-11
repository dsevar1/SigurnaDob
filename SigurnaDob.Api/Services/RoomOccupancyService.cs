using Microsoft.EntityFrameworkCore;
using SigurnaDob.Api.Data;
using SigurnaDob.Shared.Constants;

namespace SigurnaDob.Api.Services;

/// <summary>
/// Jedini izvor istine za izračun popunjenosti sobe (broj residenata sa statusom Aktivan ili
/// Privremeno odsutan trenutno dodijeljenih toj sobi). Koriste ga i RoomsController (prikaz
/// popunjenosti, provjera kapaciteta pri promjeni sobe) i ResidentsController (provjera
/// kapaciteta pri dodjeli sobe residentu) - popunjenost se nikad ne sprema kao polje.
/// </summary>
public class RoomOccupancyService
{
    private readonly SigurnaDobDbContext _db;

    public RoomOccupancyService(SigurnaDobDbContext db)
    {
        _db = db;
    }

    public async Task<int> GetOccupancyAsync(int roomId, int? excludeResidentId = null)
    {
        var relevantStatuses = ResidentStatusIds.RacunaSeUPopunjenost;

        return await _db.Residents
            .CountAsync(r => r.RoomId == roomId
                && relevantStatuses.Contains(r.ResidentStatusId)
                && (excludeResidentId == null || r.Id != excludeResidentId));
    }

    public async Task<Dictionary<int, int>> GetOccupancyByRoomAsync()
    {
        var relevantStatuses = ResidentStatusIds.RacunaSeUPopunjenost;

        return await _db.Residents
            .Where(r => r.RoomId != null && relevantStatuses.Contains(r.ResidentStatusId))
            .GroupBy(r => r.RoomId!.Value)
            .Select(g => new { RoomId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.RoomId, x => x.Count);
    }
}
