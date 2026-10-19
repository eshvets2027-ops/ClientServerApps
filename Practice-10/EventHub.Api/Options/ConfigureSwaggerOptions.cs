using Microsoft.Extensions.Options;
using Microsoft.OpenApi;

namespace EventHub.Api.Options;

public sealed class ConfigureSwaggerOptions(IOptions<OpenApiOptions> openApiOptions)
    : IConfigureOptions<Swashbuckle.AspNetCore.SwaggerGen.SwaggerGenOptions>
{
    public void Configure(Swashbuckle.AspNetCore.SwaggerGen.SwaggerGenOptions options)
    {
        var settings = openApiOptions.Value;
        options.SwaggerDoc(settings.Version, new OpenApiInfo
        {
            Title = settings.Title,
            Version = settings.Version,
            Description = settings.Description
        });
    }
}
