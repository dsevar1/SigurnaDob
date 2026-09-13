namespace SigurnaDob.Shared.DTOs.Users;

public class AppUserCreateDto
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public List<int> RoleIds { get; set; } = new();
    public int? StaffId { get; set; }
    public int? FamilyContactId { get; set; }
}
