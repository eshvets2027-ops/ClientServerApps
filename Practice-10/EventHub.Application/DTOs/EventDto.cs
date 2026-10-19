namespace EventHub.Application.DTOs;

public sealed record EventDto(int Id, string Name, string Description, DateTime StartsAtUtc, DateTime EndsAtUtc, string? CoverImageUrl, string Location, double? Latitude, double? Longitude);
