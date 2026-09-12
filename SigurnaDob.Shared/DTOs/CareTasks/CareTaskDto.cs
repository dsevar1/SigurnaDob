namespace SigurnaDob.Shared.DTOs.CareTasks;

public class CareTaskDto
{
    public int Id { get; set; }
    public int ResidentId { get; set; }
    public string ResidentFullName { get; set; } = string.Empty;
    public int CareTaskTypeId { get; set; }
    public string CareTaskTypeName { get; set; } = string.Empty;
    public int CareTaskStatusId { get; set; }
    public string CareTaskStatusName { get; set; } = string.Empty;
    public int? AssignedStaffId { get; set; }
    public string? AssignedStaffFullName { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? CompletionNote { get; set; }
}
