namespace SigurnaDob.Shared.DTOs.Overview;

public class RoomOverviewDto
{
    public int Id { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int CurrentOccupancy { get; set; }
    public int FreeSlots { get; set; }
    public string RoomStatusName { get; set; } = string.Empty;
}
