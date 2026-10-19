namespace EventHub.Application.DTOs;

public sealed record EventDto(
    int Id,
    string Title,
    string? Description,
    string Location,
    DateTimeOffset Date,
    int Capacity,
    int CategoryId,
    string CategoryName,
    int RegistrationCount,
    DateTimeOffset CreatedAt);
