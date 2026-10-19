using EventHub.Domain.Entities;

namespace EventHub.Domain.Interfaces;

public interface IEventRepository
{
    Task<Event?> GetByIdAsync(int id);
}
