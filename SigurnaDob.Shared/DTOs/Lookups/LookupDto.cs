namespace SigurnaDob.Shared.DTOs.Lookups;

public class LookupDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Opcionalno - popunjeno samo gdje je relevantno (npr. Staff kad se traže i neaktivni zapisi).
    /// Potrošači koji ga ne trebaju ga jednostavno ignoriraju.
    /// </summary>
    public bool? IsActive { get; set; }
}
