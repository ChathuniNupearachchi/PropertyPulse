namespace PropertyPulse.Infrastructure.Storage;

public interface IPropertyImageStorage
{
    Task<string> SaveAsync(Guid propertyId, Stream content, string fileExtension, CancellationToken cancellationToken = default);
    void Delete(Guid propertyId, string fileName);
    void DeleteAll(Guid propertyId);
}
