using System.ComponentModel.DataAnnotations;

namespace EventHub.Application.DTOs;

/// <summary>Дані для створення категорії.</summary>
public sealed class CreateCategoryRequest
{
    /// <summary>Назва категорії.</summary>
    [Required, MaxLength(100)]
    public string Name { get; init; } = string.Empty;
}
