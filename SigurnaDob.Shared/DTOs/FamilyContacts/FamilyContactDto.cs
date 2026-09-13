namespace SigurnaDob.Shared.DTOs.FamilyContacts;

public class FamilyContactDto
{
    public int Id { get; set; }
    public int ResidentId { get; set; }
    public string ResidentFullName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Relationship { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
}
