using PropertyPulse.Domain.Entities;

namespace PropertyPulse.Infrastructure.Repositories;

public interface IPropertyRepository
{
    Task<Property?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Property> Items, int TotalCount)> QueryAsync(PropertyQuery query, CancellationToken cancellationToken = default);
    Task AddAsync(Property property, CancellationToken cancellationToken = default);
    Task UpdateAsync(Property property, CancellationToken cancellationToken = default);
    Task AddImagesAsync(IReadOnlyCollection<PropertyImage> images, CancellationToken cancellationToken = default);
    Task DeleteAsync(Property property, CancellationToken cancellationToken = default);
}
