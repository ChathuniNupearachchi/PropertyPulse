using Microsoft.AspNetCore.Http;
using PropertyPulse.Api.Validation;

namespace PropertyPulse.Api.Tests.Validation;

public class PropertyImageValidatorTests
{
    [Fact]
    public void Validate_WithAValidJpeg_ReturnsNoErrors()
    {
        var errors = PropertyImageValidator.Validate([CreateFile("image/jpeg", 1024)], existingCount: 0);

        Assert.Empty(errors);
    }

    [Theory]
    [InlineData("image/jpeg")]
    [InlineData("image/png")]
    [InlineData("image/webp")]
    public void Validate_AcceptsEachAllowedContentType(string contentType)
    {
        var errors = PropertyImageValidator.Validate([CreateFile(contentType, 1024)], existingCount: 0);

        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_RejectsAWrongContentType()
    {
        var errors = PropertyImageValidator.Validate([CreateFile("application/pdf", 1024)], existingCount: 0);

        Assert.Single(errors);
        Assert.Contains("JPEG, PNG, or WebP", errors[0].Message);
    }

    [Fact]
    public void Validate_RejectsAFileOverTheSizeLimit()
    {
        var errors = PropertyImageValidator.Validate([CreateFile("image/jpeg", PropertyImageValidator.MaxFileSizeBytes + 1)], existingCount: 0);

        Assert.Single(errors);
        Assert.Contains("10 MB", errors[0].Message);
    }

    [Fact]
    public void Validate_AcceptsAFileExactlyAtTheSizeLimit()
    {
        var errors = PropertyImageValidator.Validate([CreateFile("image/jpeg", PropertyImageValidator.MaxFileSizeBytes)], existingCount: 0);

        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_RejectsGoingOverThePhotoLimit()
    {
        var files = Enumerable.Range(0, 3).Select(_ => CreateFile("image/jpeg", 1024)).ToList();

        var errors = PropertyImageValidator.Validate(files, existingCount: PropertyImageValidator.MaxPhotosPerProperty - 2);

        Assert.Single(errors);
        Assert.Contains("at most 10 photos", errors[0].Message);
    }

    [Fact]
    public void Validate_AcceptsExactlyReachingThePhotoLimit()
    {
        var files = Enumerable.Range(0, 2).Select(_ => CreateFile("image/jpeg", 1024)).ToList();

        var errors = PropertyImageValidator.Validate(files, existingCount: PropertyImageValidator.MaxPhotosPerProperty - 2);

        Assert.Empty(errors);
    }

    [Theory]
    [InlineData("image/jpeg", ".jpg")]
    [InlineData("image/png", ".png")]
    [InlineData("image/webp", ".webp")]
    public void GetExtension_ReturnsTheMatchingExtension(string contentType, string expectedExtension)
    {
        Assert.Equal(expectedExtension, PropertyImageValidator.GetExtension(contentType));
    }

    private static IFormFile CreateFile(string contentType, long length)
    {
        var stream = new MemoryStream(new byte[length]);
        return new FormFile(stream, 0, length, "photos", "photo.jpg") { Headers = new HeaderDictionary(), ContentType = contentType };
    }
}
