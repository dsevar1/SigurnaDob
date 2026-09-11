namespace SigurnaDob.Shared.Models;

public class AppRole
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<AppUserRole> AppUserRoles { get; set; } = new List<AppUserRole>();
}
