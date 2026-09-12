namespace SigurnaDob.Shared.Constants;

/// <summary>
/// Fiksni ID-jevi iz seed podataka za care_task_statuses (vidi SigurnaDobDbContext.OnModelCreating).
/// Koristiti umjesto magic numbers gdje god se poslovna logika grana po statusu zadatka skrbi.
/// </summary>
public static class CareTaskStatusIds
{
    public const int Novo = 1;
    public const int Dodijeljeno = 2;
    public const int UTijeku = 3;
    public const int Izvrseno = 4;
    public const int Otkazano = 5;
}
