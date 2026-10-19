using EventHub.Domain.Entities;

namespace EventHub.Domain.Interfaces;

public interface IEventRepository
{
    Task<IReadOnlyList<Event>> GetAllAsync(CancellationToken cancellationToken);
    Task<Event?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<Event> AddAsync(Event eventItem, CancellationToken cancellationToken);
    Task<bool> UpdateAsync(int id, Event eventItem, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken);
}
