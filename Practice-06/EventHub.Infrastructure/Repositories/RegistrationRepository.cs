using EventHub.Domain.Entities;
using EventHub.Domain.Interfaces;
using EventHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EventHub.Infrastructure.Repositories;

public sealed class RegistrationRepository(AppDbContext db) : IRegistrationRepository
{
    public Task<bool> ExistsAsync(int eventId, string email) =>
        db.Registrations.AnyAsync(x => x.EventId == eventId && x.AttendeeEmail.ToLower() == email.ToLower() && x.Status != RegistrationStatus.Cancelled);

    public Task<int> CountActiveByEventIdAsync(int eventId) =>
        db.Registrations.CountAsync(x => x.EventId == eventId && x.Status != RegistrationStatus.Cancelled);

    public async Task AddAsync(Registration registration)
    {
        db.Registrations.Add(registration);
        await db.SaveChangesAsync();
    }
}
