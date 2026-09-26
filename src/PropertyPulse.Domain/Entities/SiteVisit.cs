namespace PropertyPulse.Domain.Entities;

public class SiteVisit : BaseEntity
{
    public Guid LeadId { get; set; }
    public Lead? Lead { get; set; }

    public Guid PropertyId { get; set; }
    public Property? Property { get; set; }

    public Guid AgentId { get; set; }
    public User? Agent { get; set; }

    public DateTime ScheduledAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? Notes { get; set; }
}
