using Microsoft.Extensions.Options;

namespace PropertyPulse.Infrastructure.Storage;

public class PropertyImageStorage(IOptions<ImageStorageOptions> options) : IPropertyImageStorage
{
    private readonly string _rootPath = options.Value.RootPath;

    public async Task<string> SaveAsync(
        Guid propertyId, Stream content, string fileExtension, CancellationToken cancellationToken = default)
    {
        var directory = GetPropertyDirectory(propertyId);
        Directory.CreateDirectory(directory);

        var fileName = $"{Guid.NewGuid()}{fileExtension}";
        var path = Path.Combine(directory, fileName);

        await using var fileStream = File.Create(path);
        await content.CopyToAsync(fileStream, cancellationToken);

        return fileName;
    }

    public void Delete(Guid propertyId, string fileName)
    {
        var path = Path.Combine(GetPropertyDirectory(propertyId), fileName);
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    public void DeleteAll(Guid propertyId)
    {
        var directory = GetPropertyDirectory(propertyId);
        if (Directory.Exists(directory))
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    private string GetPropertyDirectory(Guid propertyId) => Path.Combine(_rootPath, propertyId.ToString());
}
