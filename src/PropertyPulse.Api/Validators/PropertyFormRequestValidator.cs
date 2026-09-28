using FluentValidation;
using PropertyPulse.Domain.Enums;
using PropertyPulse.Shared.Properties;

namespace PropertyPulse.Api.Validators;

public class PropertyFormRequestValidator : AbstractValidator<PropertyFormRequest>
{
    public PropertyFormRequestValidator()
    {
        RuleFor(r => r.Title).NotEmpty().MaximumLength(200);
        RuleFor(r => r.Description).NotEmpty().MaximumLength(4000);
        RuleFor(r => r.Address).NotEmpty().MaximumLength(300);
        RuleFor(r => r.City).NotEmpty().MaximumLength(100);

        RuleFor(r => r.PropertyType).Must(v => Enum.TryParse<PropertyType>(v, out _))
            .WithMessage($"'Property Type' must be one of: {string.Join(", ", Enum.GetNames<PropertyType>())}.");
        RuleFor(r => r.Status).Must(v => Enum.TryParse<PropertyStatus>(v, out _))
            .WithMessage($"'Status' must be one of: {string.Join(", ", Enum.GetNames<PropertyStatus>())}.");

        RuleFor(r => r.PriceLkr).GreaterThan(0);
        RuleFor(r => r.Bedrooms).GreaterThanOrEqualTo(0);
        RuleFor(r => r.Bathrooms).GreaterThanOrEqualTo(0);
        RuleFor(r => r.FloorAreaSqFt).GreaterThan(0).When(r => r.FloorAreaSqFt is not null);
        RuleFor(r => r.LandSizePerches).GreaterThan(0).When(r => r.LandSizePerches is not null);
        RuleFor(r => r.Latitude).InclusiveBetween(-90, 90).When(r => r.Latitude is not null);
        RuleFor(r => r.Longitude).InclusiveBetween(-180, 180).When(r => r.Longitude is not null);
    }
}
