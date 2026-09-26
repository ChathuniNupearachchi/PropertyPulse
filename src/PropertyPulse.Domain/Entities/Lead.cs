using PropertyPulse.Domain.Enums;

namespace PropertyPulse.Domain.Entities;

public class Lead : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public LeadStage Stage { get; set; }
    public decimal? BudgetLkr { get; set; }

    public Guid AgentId { get; set; }
    public User? Agent { get; set; }

    public Guid? PropertyId { get; set; }
    public Property? Property { get; set; }

    public List<LeadNote> Notes { get; set; } = [];
}
