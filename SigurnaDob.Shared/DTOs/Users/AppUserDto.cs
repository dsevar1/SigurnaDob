namespace SigurnaDob.Shared.DTOs.Users;

public class AppUserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public List<string> Roles { get; set; } = new();
    public int? StaffId { get; set; }
    public string? StaffFullName { get; set; }
    public int? FamilyContactId { get; set; }
    public string? FamilyContactFullName { get; set; }
}
