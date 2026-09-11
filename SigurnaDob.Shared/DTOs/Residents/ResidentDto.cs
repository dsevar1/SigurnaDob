namespace SigurnaDob.Shared.DTOs.Residents;

public class ResidentDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public DateOnly? DateOfBirth { get; set; }
    public int ResidentStatusId { get; set; }
    public string ResidentStatusName { get; set; } = string.Empty;
    public int? RoomId { get; set; }
    public string? RoomNumber { get; set; }
    public DateOnly? AdmissionDate { get; set; }
}
