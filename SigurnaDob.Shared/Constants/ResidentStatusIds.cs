namespace SigurnaDob.Shared.Constants;

/// <summary>
/// Fiksni ID-jevi iz seed podataka za resident_statuses (vidi SigurnaDobDbContext.OnModelCreating).
/// Koristiti umjesto magic numbers gdje god se poslovna logika grana po statusu korisnika doma.
/// </summary>
public static class ResidentStatusIds
{
    public const int UPripremiZaPrijem = 1;
    public const int Aktivan = 2;
    public const int PrivremenoOdsutan = 3;
    public const int PremjestenIzDoma = 4;
    public const int Arhiviran = 5;

    /// <summary>Statusi koji se računaju u trenutnu popunjenost sobe.</summary>
    public static readonly int[] RacunaSeUPopunjenost = [Aktivan, PrivremenoOdsutan];
}
