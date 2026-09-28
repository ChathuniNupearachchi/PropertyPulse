namespace PropertyPulse.Infrastructure.Storage;

public class ImageStorageOptions
{
    public const string SectionName = "ImageStorage";

    public string RootPath { get; set; } = string.Empty;
}
