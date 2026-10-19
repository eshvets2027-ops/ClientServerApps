using System.ComponentModel.DataAnnotations;

namespace EventHub.Application.DTOs;

public sealed class CreateRegistrationRequest
{
    [Required]
    [StringLength(120, MinimumLength = 2)]
    public string AttendeeName { get; init; } = "";

    [Required]
    [EmailAddress]
    [StringLength(254)]
    public string AttendeeEmail { get; init; } = "";
}
