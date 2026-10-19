using EventHub.Application.DTOs;
using EventHub.Application.Exceptions;
using EventHub.Application.Validation;

namespace EventHub.Tests;

public sealed class CoverImageUploadValidatorTests
{
    private readonly CoverImageUploadValidator _validator = new(5);

    [Theory]
    [InlineData("poster.jpg", "image/jpeg", ".jpg")]
    [InlineData("poster.jpeg", "image/jpeg", ".jpeg")]
    [InlineData("poster.png", "image/png", ".png")]
    public void ValidateAndGetExtension_WithSupportedImage_ReturnsNormalizedExtension(string name, string type, string expected)
    {
        using var stream = new MemoryStream([1, 2, 3]);
        var upload = new CoverImageUpload(stream, name, type, 3);
        Assert.Equal(expected, _validator.ValidateAndGetExtension(upload));
    }

    [Fact]
    public void ValidateAndGetExtension_WithEmptyFile_Throws()
    {
        using var stream = new MemoryStream();
        var upload = new CoverImageUpload(stream, "poster.png", "image/png", 0);
        Assert.Throws<InvalidFileUploadException>(() => _validator.ValidateAndGetExtension(upload));
    }

    [Fact]
    public void ValidateAndGetExtension_WithUnsupportedType_Throws()
    {
        using var stream = new MemoryStream([1]);
        var upload = new CoverImageUpload(stream, "notes.txt", "text/plain", 1);
        Assert.Throws<InvalidFileUploadException>(() => _validator.ValidateAndGetExtension(upload));
    }

    [Fact]
    public void ValidateAndGetExtension_WithMismatchedExtension_Throws()
    {
        using var stream = new MemoryStream([1]);
        var upload = new CoverImageUpload(stream, "poster.jpg", "image/png", 1);
        Assert.Throws<InvalidFileUploadException>(() => _validator.ValidateAndGetExtension(upload));
    }

    [Fact]
    public void ValidateAndGetExtension_WithOversizedFile_Throws()
    {
        using var stream = new MemoryStream([1]);
        var upload = new CoverImageUpload(stream, "poster.png", "image/png", 5L * 1024 * 1024 + 1);
        Assert.Throws<InvalidFileUploadException>(() => _validator.ValidateAndGetExtension(upload));
    }
}
