namespace SigurnaDob.Shared.DTOs.Auth;

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public string Username { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public int? StaffId { get; set; }
    public int? FamilyContactId { get; set; }
}
