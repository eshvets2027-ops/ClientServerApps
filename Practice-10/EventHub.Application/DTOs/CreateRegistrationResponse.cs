namespace EventHub.Application.DTOs;

public sealed record CreateRegistrationResponse(RegistrationDto Registration, Guid JobId);
