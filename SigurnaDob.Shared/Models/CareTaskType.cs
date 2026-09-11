namespace SigurnaDob.Shared.Models;

public class CareTaskType
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<CareTask> CareTasks { get; set; } = new List<CareTask>();
}
