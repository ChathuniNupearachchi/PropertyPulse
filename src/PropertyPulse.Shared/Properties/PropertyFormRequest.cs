namespace PropertyPulse.Shared.Properties;

public record PropertyFormRequest(
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
    double? LandSizePerches);
