using Microsoft.Extensions.Options;

namespace EventHub.Api.Options;

public sealed class OpenApiOptionsValidator : IValidateOptions<OpenApiOptions>
{
    public ValidateOptionsResult Validate(string? name, OpenApiOptions options)
    {
        var failures = new List<string>();

        if (string.IsNullOrWhiteSpace(options.Title))
            failures.Add("OpenApi:Title is required.");

        if (string.IsNullOrWhiteSpace(options.Version))
            failures.Add("OpenApi:Version is required.");

        return failures.Count > 0
            ? ValidateOptionsResult.Fail(failures)
            : ValidateOptionsResult.Success;
    }
}
