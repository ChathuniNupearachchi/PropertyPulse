namespace PropertyPulse.Domain.Entities;

public class LeadNote : BaseEntity
{
    public Guid LeadId { get; set; }
    public Lead? Lead { get; set; }

    public Guid AuthorId { get; set; }
    public User? Author { get; set; }

    public string Text { get; set; } = string.Empty;
}
