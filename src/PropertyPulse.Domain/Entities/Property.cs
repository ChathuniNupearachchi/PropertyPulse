using PropertyPulse.Domain.Enums;

namespace PropertyPulse.Domain.Entities;

public class Property : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public PropertyType PropertyType { get; set; }
    public PropertyStatus Status { get; set; }
    public decimal PriceLkr { get; set; }
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public int Bedrooms { get; set; }
    public int Bathrooms { get; set; }
    public double? FloorAreaSqFt { get; set; }
    public double? LandSizePerches { get; set; }

    public Guid AgentId { get; set; }
    public User? Agent { get; set; }

    public List<PropertyImage> Images { get; set; } = [];
}
