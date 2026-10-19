using EventHub.Application.DTOs;
using EventHub.Domain.Entities;
using EventHub.Domain.Interfaces;

namespace EventHub.Application.Services;

public sealed class RegistrationService(
    IEventRepository eventRepository,
    IRegistrationRepository registrationRepository)
{
    public async Task<RegistrationDto> CreateAsync(int eventId, CreateRegistrationRequest request)
    {
        var eventItem = await eventRepository.GetByIdAsync(eventId);
        if (eventItem is null)
            throw new RegistrationNotFoundException();

        var email = request.AttendeeEmail.Trim().ToLowerInvariant();
        if (await registrationRepository.ExistsAsync(eventId, email))
            throw new RegistrationConflictException();

        var activeCount = await registrationRepository.CountActiveByEventIdAsync(eventId);
        if (activeCount >= eventItem.Capacity)
            throw new RegistrationCapacityException();

        var registration = new Registration
        {
            EventId = eventId,
            AttendeeName = request.AttendeeName.Trim(),
            AttendeeEmail = email,
            Status = RegistrationStatus.Pending
        };

        await registrationRepository.AddAsync(registration);
        return new RegistrationDto(registration.Id, eventId, registration.AttendeeName, registration.AttendeeEmail, "pending");
    }
}

public sealed class RegistrationNotFoundException : Exception { }
public sealed class RegistrationConflictException : Exception { }
public sealed class RegistrationCapacityException : Exception { }
