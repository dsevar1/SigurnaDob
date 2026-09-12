namespace SigurnaDob.Shared.DTOs.FamilyContacts;

public class FamilyContactUpdateDto
{
    public string FullName { get; set; } = string.Empty;
    public string? Relationship { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
}
