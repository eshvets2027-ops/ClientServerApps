using System.ComponentModel.DataAnnotations;

namespace EventHub.Application.DTOs;

/// <summary>Параметри фільтрації та пагінації списку подій.</summary>
public sealed class EventQueryParameters : IValidatableObject
{
    /// <summary>Початкова дата фільтра включно.</summary>
    public DateOnly? DateFrom { get; init; }
    /// <summary>Кінцева дата фільтра включно.</summary>
    public DateOnly? DateTo { get; init; }
    /// <summary>Номер сторінки, починаючи з 1.</summary>
    [Range(1, int.MaxValue)]
    public int Page { get; init; } = 1;
    /// <summary>Кількість елементів на сторінці від 1 до 100.</summary>
    [Range(1, 100)]
    public int PageSize { get; init; } = 10;
    /// <summary>Пошук за назвою події.</summary>
    [MaxLength(200)]
    public string? Search { get; init; }

    /// <inheritdoc />
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (DateFrom.HasValue && DateTo.HasValue && DateFrom.Value > DateTo.Value)
            yield return new ValidationResult("DateFrom must be earlier than or equal to DateTo.", [nameof(DateFrom), nameof(DateTo)]);
    }
}
