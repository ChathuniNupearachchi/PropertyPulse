using PropertyPulse.Domain.Enums;

namespace PropertyPulse.Infrastructure.Repositories;

public record PropertyQuery(
    PropertyType? PropertyType,
    PropertyStatus? Status,
    string? City,
    decimal? MinPrice,
    decimal? MaxPrice,
    string? Search,
    int Page,
    int PageSize);
