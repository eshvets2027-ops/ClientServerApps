using System.ComponentModel.DataAnnotations;
namespace EventHub.Application.Validation;
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed class FutureDateAttribute : ValidationAttribute
{
    public FutureDateAttribute() : base("The {0} field must be a date in the future.") { }
    public override bool IsValid(object? value) => value is null || value is DateTimeOffset date && date > DateTimeOffset.UtcNow;
}
