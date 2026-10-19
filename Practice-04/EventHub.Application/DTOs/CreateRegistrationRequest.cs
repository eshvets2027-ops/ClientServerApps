using System.ComponentModel.DataAnnotations;
namespace EventHub.Application.DTOs;
public sealed record CreateRegistrationRequest([Required, EmailAddress, MaxLength(320)] string AttendeeEmail);
