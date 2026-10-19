using EventHub.Domain.Entities;
using EventHub.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EventHub.Infrastructure.Persistence.Repositories;

public sealed class EventRepository(AppDbContext db) : IEventRepository
{
    public async Task<IReadOnlyList<Event>> GetAllAsync(CancellationToken ct) =>
        await db.Events.AsNoTracking().OrderBy(x => x.Id).ToListAsync(ct);

    public Task<Event?> GetByIdAsync(int id, CancellationToken ct) =>
        db.Events.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<Event> AddAsync(Event eventItem, CancellationToken ct)
    {
        db.Events.Add(eventItem);
        await db.SaveChangesAsync(ct);
        return eventItem;
    }

    public async Task<bool> UpdateAsync(int id, Event item, CancellationToken ct)
    {
        var existing = await db.Events.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (existing is null) return false;
        existing.Title = item.Title;
        existing.Description = item.Description;
        existing.Location = item.Location;
        existing.Organizer = item.Organizer;
        existing.Date = item.Date;
        existing.Capacity = item.Capacity;
        existing.CategoryId = item.CategoryId;
        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct)
    {
        var existing = await db.Events.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (existing is null) return false;
        db.Events.Remove(existing);
        await db.SaveChangesAsync(ct);
        return true;
    }
}
