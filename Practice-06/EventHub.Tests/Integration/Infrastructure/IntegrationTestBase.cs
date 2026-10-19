using EventHub.Domain.Entities;
using EventHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EventHub.Tests.Integration.Infrastructure;

public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected IntegrationTestBase(EventHubWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
    }

    protected EventHubWebApplicationFactory Factory { get; }
    protected HttpClient Client { get; }

    public Task InitializeAsync() => Factory.ResetDatabaseAsync();

    public Task DisposeAsync()
    {
        Client.Dispose();
        return Task.CompletedTask;
    }

    protected Task<int> SeedEventAsync(int capacity = 20)
    {
        return Factory.ExecuteDbContextAsync(async db =>
        {
            var category = new Category { Name = "Meetups" };
            var eventItem = new Event
            {
                Title = "ASP.NET Core meetup",
                Location = "Kharkiv",
                Date = DateTimeOffset.UtcNow.AddDays(30),
                Capacity = capacity,
                Category = category
            };
            db.Events.Add(eventItem);
            await db.SaveChangesAsync();
            return eventItem.Id;
        });
    }

    protected Task SeedRegistrationAsync(int eventId, string email, RegistrationStatus status = RegistrationStatus.Pending)
    {
        return Factory.ExecuteDbContextAsync(async db =>
        {
            db.Registrations.Add(new Registration
            {
                EventId = eventId,
                AttendeeName = "Existing attendee",
                AttendeeEmail = email,
                Status = status
            });
            await db.SaveChangesAsync();
        });
    }

    protected Task<int> RegistrationCountAsync()
    {
        return Factory.ExecuteDbContextAsync(db => db.Registrations.CountAsync());
    }
}
