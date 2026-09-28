namespace PropertyPulse.Shared.Properties;

public record PropertyDto(
    Guid Id,
    string Title,
    string Description,
    string PropertyType,
    string Status,
    decimal PriceLkr,
    string Address,
    string City,
    double? Latitude,
    double? Longitude,
    int Bedrooms,
    int Bathrooms,
    double? FloorAreaSqFt,
    double? LandSizePerches,
    Guid AgentId,
    string AgentName,
    List<PropertyImageDto> Images);
