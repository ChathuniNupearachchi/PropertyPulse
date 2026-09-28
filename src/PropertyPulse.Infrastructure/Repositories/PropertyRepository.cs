using Microsoft.EntityFrameworkCore;
using PropertyPulse.Domain.Entities;
using PropertyPulse.Infrastructure.Data;

namespace PropertyPulse.Infrastructure.Repositories;

public class PropertyRepository(AppDbContext dbContext) : IPropertyRepository
{
    public Task<Property?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Properties
            .Include(p => p.Images)
            .Include(p => p.Agent)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<(IReadOnlyList<Property> Items, int TotalCount)> QueryAsync(
        PropertyQuery query, CancellationToken cancellationToken = default)
    {
        var filtered = dbContext.Properties.AsNoTracking().AsQueryable();

        if (query.PropertyType is not null)
        {
            filtered = filtered.Where(p => p.PropertyType == query.PropertyType);
        }

        if (query.Status is not null)
        {
            filtered = filtered.Where(p => p.Status == query.Status);
        }

        if (!string.IsNullOrWhiteSpace(query.City))
        {
            filtered = filtered.Where(p => EF.Functions.ILike(p.City, $"%{query.City}%"));
        }

        if (query.MinPrice is not null)
        {
            filtered = filtered.Where(p => p.PriceLkr >= query.MinPrice);
        }

        if (query.MaxPrice is not null)
        {
            filtered = filtered.Where(p => p.PriceLkr <= query.MaxPrice);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = $"%{query.Search}%";
            filtered = filtered.Where(p =>
                EF.Functions.ILike(p.Title, term) ||
                EF.Functions.ILike(p.Address, term) ||
                EF.Functions.ILike(p.City, term));
        }

        var totalCount = await filtered.CountAsync(cancellationToken);

        var items = await filtered
            .Include(p => p.Images)
            .Include(p => p.Agent)
            .OrderByDescending(p => p.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task AddAsync(Property property, CancellationToken cancellationToken = default)
    {
        dbContext.Properties.Add(property);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task UpdateAsync(Property property, CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);

    public async Task AddImagesAsync(IReadOnlyCollection<PropertyImage> images, CancellationToken cancellationToken = default)
    {
        // Added explicitly rather than through property.Images.Add(...): a new child appended to an already-tracked
        // (Unchanged) parent's collection is otherwise picked up as Modified, not Added, because its client-generated
        // Guid key already looks "set" - EF Core then issues an UPDATE that matches no row instead of an INSERT.
        dbContext.PropertyImages.AddRange(images);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Property property, CancellationToken cancellationToken = default)
    {
        dbContext.Properties.Remove(property);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
