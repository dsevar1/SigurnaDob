namespace SigurnaDob.Shared.DTOs.Rooms;

public class RoomDto
{
    public int Id { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int RoomStatusId { get; set; }
    public string RoomStatusName { get; set; } = string.Empty;
    public int CurrentOccupancy { get; set; }
    public int FreeCapacity { get; set; }
}
