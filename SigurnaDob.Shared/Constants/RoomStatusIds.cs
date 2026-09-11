namespace SigurnaDob.Shared.Constants;

/// <summary>
/// Fiksni ID-jevi iz seed podataka za room_statuses (vidi SigurnaDobDbContext.OnModelCreating).
/// Koristiti umjesto magic numbers gdje god se poslovna logika grana po statusu sobe.
/// </summary>
public static class RoomStatusIds
{
    public const int UUporabi = 1;
    public const int Odrzavanje = 2;
    public const int IzvanUporabe = 3;

    /// <summary>Statusi sobe koji zabranjuju dodjelu korisnika doma toj sobi.</summary>
    public static readonly int[] ZabranjujuDodjelu = [Odrzavanje, IzvanUporabe];
}
