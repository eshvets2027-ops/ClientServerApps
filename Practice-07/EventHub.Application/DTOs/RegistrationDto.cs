namespace EventHub.Application.DTOs;

public sealed record RegistrationDto(int Id, int EventId, string AttendeeName, string AttendeeEmail, DateTime RegisteredAtUtc);
