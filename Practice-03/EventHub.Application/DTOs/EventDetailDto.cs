namespace EventHub.Application.DTOs;

public sealed record EventDetailDto(
    int Id,
    string Title,
    string? Description,
    string Location,
    DateTimeOffset Date,
    int Capacity,
    int CategoryId,
    string CategoryName,
    DateTimeOffset CreatedAt,
    IReadOnlyList<RegistrationDto> Registrations);
