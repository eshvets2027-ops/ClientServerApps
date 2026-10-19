using EventHub.Domain.Entities;

namespace EventHub.Domain.Interfaces;

public interface IRegistrationRepository
{
    Task<bool> ExistsAsync(int eventId, string email);
    Task<int> CountActiveByEventIdAsync(int eventId);
    Task AddAsync(Registration registration);
}
