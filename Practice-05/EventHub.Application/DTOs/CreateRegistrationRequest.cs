using System.ComponentModel.DataAnnotations;

namespace EventHub.Application.DTOs;

/// <summary>Дані для реєстрації учасника.</summary>
public sealed class CreateRegistrationRequest
{
    /// <summary>Email учасника.</summary>
    [Required, EmailAddress, MaxLength(320)]
    public string AttendeeEmail { get; init; } = string.Empty;
}
