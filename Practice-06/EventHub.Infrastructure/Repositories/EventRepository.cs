using EventHub.Domain.Entities;
using EventHub.Domain.Interfaces;
using EventHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EventHub.Infrastructure.Repositories;

public sealed class EventRepository(AppDbContext db) : IEventRepository
{
    public Task<Event?> GetByIdAsync(int id) => db.Events.SingleOrDefaultAsync(x => x.Id == id);
}
