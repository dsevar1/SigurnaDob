namespace SigurnaDob.Shared.Models;

public class Room
{
    public int Id { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int RoomStatusId { get; set; }

    public RoomStatus? RoomStatus { get; set; }
    public ICollection<Resident> Residents { get; set; } = new List<Resident>();
}
