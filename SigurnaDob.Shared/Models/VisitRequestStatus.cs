namespace SigurnaDob.Shared.Models;

public class VisitRequestStatus
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<VisitRequest> VisitRequests { get; set; } = new List<VisitRequest>();
}
