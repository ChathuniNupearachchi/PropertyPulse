namespace PropertyPulse.Api.Validation;

public static class PropertyImageValidator
{
    public const long MaxFileSizeBytes = 10 * 1024 * 1024;
    public const int MaxPhotosPerProperty = 10;

    private static readonly Dictionary<string, string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        ["image/jpeg"] = ".jpg",
        ["image/png"] = ".png",
        ["image/webp"] = ".webp"
    };

    public static List<(string Field, string Message)> Validate(IReadOnlyList<IFormFile> files, int existingCount)
    {
        var errors = new List<(string Field, string Message)>();

        if (existingCount + files.Count > MaxPhotosPerProperty)
        {
            errors.Add(("Photos", $"A property can have at most {MaxPhotosPerProperty} photos."));
            return errors;
        }

        for (var i = 0; i < files.Count; i++)
        {
            var field = $"Photos[{i}]";
            var file = files[i];

            if (!AllowedContentTypes.ContainsKey(file.ContentType))
            {
                errors.Add((field, "Photos must be JPEG, PNG, or WebP images."));
                continue;
            }

            if (file.Length > MaxFileSizeBytes)
            {
                errors.Add((field, "Each photo must be at most 10 MB."));
            }
        }

        return errors;
    }

    public static string GetExtension(string contentType) => AllowedContentTypes[contentType];
}
