namespace SigurnaDob.Shared.DTOs.Rooms;

public class RoomCreateDto
{
    public string RoomNumber { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int RoomStatusId { get; set; }
}
