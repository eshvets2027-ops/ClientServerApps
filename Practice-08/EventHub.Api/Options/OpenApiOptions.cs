namespace EventHub.Api.Options;

public sealed class OpenApiOptions
{
    public const string SectionName = "OpenApi";
    public string Title { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
