using EventHub.Api.Options;
using Microsoft.Extensions.Options;
using Xunit;

namespace EventHub.Tests;

public sealed class OpenApiOptionsValidatorTests
{
    private readonly OpenApiOptionsValidator _validator = new();

    [Fact]
    public void Validate_ReturnsSuccess_WhenTitleAndVersionAreProvided()
    {
        var result = _validator.Validate(Options.DefaultName, new OpenApiOptions
        {
            Title = "EventHub API",
            Version = "v1",
            Description = "EventHub API"
        });

        Assert.Equal(ValidateOptionsResult.Success, result);
    }

    [Theory]
    [InlineData("", "v1")]
    [InlineData("EventHub API", "")]
    [InlineData(" ", "v1")]
    [InlineData("EventHub API", " ")]
    public void Validate_ReturnsFailure_WhenTitleOrVersionIsMissing(string title, string version)
    {
        var result = _validator.Validate(Options.DefaultName, new OpenApiOptions
        {
            Title = title,
            Version = version
        });

        Assert.False(result.Succeeded);
    }
}
