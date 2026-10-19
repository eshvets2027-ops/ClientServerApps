using System.ComponentModel.DataAnnotations;
using EventHub.Application.Validation;

namespace EventHub.Application.DTOs;

/// <summary>Параметри оновлення події.</summary>
public sealed class UpdateEventRequest
{
    /// <summary>Назва події.</summary>
    [Required, MaxLength(200)]
    public string Title { get; init; } = string.Empty;
    /// <summary>Опис події.</summary>
    [MaxLength(4000)]
    public string? Description { get; init; }
    /// <summary>Місце проведення.</summary>
    [Required, MaxLength(500)]
    public string Location { get; init; } = string.Empty;
    /// <summary>Дата й час проведення в майбутньому.</summary>
    [FutureDate]
    public DateTimeOffset Date { get; init; }
    /// <summary>Максимальна кількість учасників.</summary>
    [Range(1, 10000)]
    public int Capacity { get; init; }
    /// <summary>Ідентифікатор категорії.</summary>
    [Range(1, int.MaxValue)]
    public int CategoryId { get; init; }
}
