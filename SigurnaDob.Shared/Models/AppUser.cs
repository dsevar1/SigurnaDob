namespace SigurnaDob.Shared.Models;

public class AppUser
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public int? StaffId { get; set; }
    public int? FamilyContactId { get; set; }
    public DateTime CreatedAt { get; set; }

    public Staff? Staff { get; set; }
    public FamilyContact? FamilyContact { get; set; }
    public ICollection<AppUserRole> AppUserRoles { get; set; } = new List<AppUserRole>();
}
