using PropertyPulse.Domain.Entities;
using PropertyPulse.Domain.Enums;
using PropertyPulse.Shared.Properties;

namespace PropertyPulse.Api.Mappings;

public static class PropertyMappings
{
    public static Property ToEntity(this PropertyFormRequest request)
    {
        var property = new Property();
        request.ApplyTo(property);
        return property;
    }

    public static void ApplyTo(this PropertyFormRequest request, Property property)
    {
        property.Title = request.Title;
        property.Description = request.Description;
        property.PropertyType = Enum.Parse<PropertyType>(request.PropertyType);
        property.Status = Enum.Parse<PropertyStatus>(request.Status);
        property.PriceLkr = request.PriceLkr;
        property.Address = request.Address;
        property.City = request.City;
        property.Latitude = request.Latitude;
        property.Longitude = request.Longitude;
        property.Bedrooms = request.Bedrooms;
        property.Bathrooms = request.Bathrooms;
        property.FloorAreaSqFt = request.FloorAreaSqFt;
        property.LandSizePerches = request.LandSizePerches;
    }

    public static PropertyDto ToDto(this Property property, string requestBaseUrl) => new(
        property.Id,
        property.Title,
        property.Description,
        property.PropertyType.ToString(),
        property.Status.ToString(),
        property.PriceLkr,
        property.Address,
        property.City,
        property.Latitude,
        property.Longitude,
        property.Bedrooms,
        property.Bathrooms,
        property.FloorAreaSqFt,
        property.LandSizePerches,
        property.AgentId,
        property.Agent?.FullName ?? string.Empty,
        property.Images
            .OrderBy(i => i.SortOrder)
            .Select(i => i.ToDto(requestBaseUrl, property.Id))
            .ToList());

    public static PropertyImageDto ToDto(this PropertyImage image, string requestBaseUrl, Guid propertyId) =>
        new(image.Id, $"{requestBaseUrl}/images/properties/{propertyId}/{image.FilePath}", image.SortOrder);
}
