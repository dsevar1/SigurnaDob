namespace SigurnaDob.Shared.DTOs.Residents;

public class ResidentUpdateDto
{
    public string FullName { get; set; } = string.Empty;
    public DateOnly? DateOfBirth { get; set; }
    public int ResidentStatusId { get; set; }
    public int? RoomId { get; set; }
    public DateOnly? AdmissionDate { get; set; }
}
