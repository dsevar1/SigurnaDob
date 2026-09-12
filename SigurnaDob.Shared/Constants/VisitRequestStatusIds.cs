namespace SigurnaDob.Shared.Constants;

/// <summary>
/// Fiksni ID-jevi iz seed podataka za visit_request_statuses (vidi SigurnaDobDbContext.OnModelCreating).
/// Koristiti umjesto magic numbers gdje god se poslovna logika grana po statusu zahtjeva za posjet.
/// </summary>
public static class VisitRequestStatusIds
{
    public const int Zaprimljeno = 1;
    public const int Odobreno = 2;
    public const int Odbijeno = 3;
    public const int Odrzano = 4;
    public const int Otkazano = 5;
}
