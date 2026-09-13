namespace SigurnaDob.Shared.DTOs.Overview;

public class StaffWorkloadDto
{
    public int Id { get; set; }
    public string StaffFullName { get; set; } = string.Empty;
    public int OpenCareTasksCount { get; set; }
}
