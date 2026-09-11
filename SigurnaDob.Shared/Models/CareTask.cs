namespace SigurnaDob.Shared.Models;

public class CareTask
{
    public int Id { get; set; }
    public int ResidentId { get; set; }
    public int CareTaskTypeId { get; set; }
    public int CareTaskStatusId { get; set; }
    public int? AssignedStaffId { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? CompletionNote { get; set; }

    public Resident? Resident { get; set; }
    public CareTaskType? CareTaskType { get; set; }
    public CareTaskStatus? CareTaskStatus { get; set; }
    public Staff? AssignedStaff { get; set; }
}
