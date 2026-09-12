namespace SigurnaDob.Shared.DTOs.CareTasks;

public class CareTaskUpdateDto
{
    public int ResidentId { get; set; }
    public int CareTaskTypeId { get; set; }
    public int CareTaskStatusId { get; set; }
    public DateTime DueDate { get; set; }
    public int? AssignedStaffId { get; set; }
}
