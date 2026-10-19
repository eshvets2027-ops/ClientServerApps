namespace EventHub.Application.DTOs;

public sealed record JobStatusDto(Guid JobId, string Status);
