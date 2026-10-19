namespace EventHub.Application.DTOs;

public sealed record EventDetailDto(int Id, string Name, string Description, DateTime StartsAtUtc, DateTime EndsAtUtc, string? CoverImageUrl, IReadOnlyList<RegistrationDto> Registrations);
