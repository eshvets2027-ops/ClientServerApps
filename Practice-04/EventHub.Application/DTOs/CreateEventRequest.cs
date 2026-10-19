using System.ComponentModel.DataAnnotations;
using EventHub.Application.Validation;
namespace EventHub.Application.DTOs;
public sealed record CreateEventRequest(
    [Required, MaxLength(200)] string Title,
    [MaxLength(4000)] string? Description,
    [Required, MaxLength(500)] string Location,
    [FutureDate] DateTimeOffset Date,
    [Range(1, 10000)] int Capacity,
    [Range(1, int.MaxValue)] int CategoryId);
