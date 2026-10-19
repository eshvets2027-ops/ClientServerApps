using System.ComponentModel.DataAnnotations;
namespace EventHub.Application.DTOs;
public sealed class EventQueryParameters : IValidatableObject
{
    public DateOnly? DateFrom { get; init; }
    public DateOnly? DateTo { get; init; }
    [Range(1, int.MaxValue)] public int Page { get; init; } = 1;
    [Range(1, 100)] public int PageSize { get; init; } = 10;
    public string? Search { get; init; }
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (DateFrom.HasValue && DateTo.HasValue && DateFrom.Value > DateTo.Value)
            yield return new ValidationResult("DateFrom must be earlier than or equal to DateTo.", [nameof(DateFrom), nameof(DateTo)]);
    }
}
