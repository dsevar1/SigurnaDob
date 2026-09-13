namespace SigurnaDob.Shared.DTOs.Users;

public class AppUserUpdateDto
{
    public List<int> RoleIds { get; set; } = new();
    public int? StaffId { get; set; }
    public int? FamilyContactId { get; set; }
}
