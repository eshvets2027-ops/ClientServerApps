using System.Net;
using System.Net.Http.Json;
using EventHub.Application.DTOs;
using EventHub.Domain.Entities;
using EventHub.Tests.Integration.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EventHub.Tests.Integration;

[Collection(IntegrationTestCollection.Name)]
public sealed class RegistrationEndpointsTests(EventHubWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task CreateRegistration_WithFreeSeat_ReturnsCreatedAndPersistsNormalizedData()
    {
        var eventId = await SeedEventAsync();
        var response = await Client.PostAsJsonAsync($"/events/{eventId}/registrations",
            new CreateRegistrationRequest { AttendeeName = " Oksana ", AttendeeEmail = " OKSANA@EXAMPLE.COM " });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal($"/events/{eventId}/registrations", response.Headers.Location?.AbsolutePath);
        var body = await response.Content.ReadFromJsonAsync<RegistrationDto>();
        Assert.NotNull(body);
        Assert.Equal("Oksana", body.AttendeeName);
        Assert.Equal("oksana@example.com", body.AttendeeEmail);
        Assert.Equal("pending", body.Status);
        var saved = await Factory.ExecuteDbContextAsync(db => db.Registrations.AsNoTracking().SingleAsync());
        Assert.Equal("oksana@example.com", saved.AttendeeEmail);
    }

    [Fact]
    public async Task CreateRegistration_WhenEmailAlreadyRegistered_ReturnsConflictWithoutDuplicate()
    {
        var eventId = await SeedEventAsync();
        await SeedRegistrationAsync(eventId, "student@example.com");
        var response = await Client.PostAsJsonAsync($"/events/{eventId}/registrations",
            new CreateRegistrationRequest { AttendeeName = "Same attendee", AttendeeEmail = " STUDENT@EXAMPLE.COM " });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var count = await RegistrationCountAsync();
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task CreateRegistration_WhenEventDoesNotExist_ReturnsNotFound()
    {
        var response = await Client.PostAsJsonAsync("/events/99999/registrations",
            new CreateRegistrationRequest { AttendeeName = "Oksana", AttendeeEmail = "oksana@example.com" });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateRegistration_WhenCapacityIsReached_ReturnsUnprocessableEntityAndDoesNotAddRecord()
    {
        var eventId = await SeedEventAsync(capacity: 1);
        await SeedRegistrationAsync(eventId, "existing@example.com");
        var before = await RegistrationCountAsync();

        var response = await Client.PostAsJsonAsync($"/events/{eventId}/registrations",
            new CreateRegistrationRequest { AttendeeName = "New attendee", AttendeeEmail = "new@example.com" });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal("Capacity reached", problem.Title);
        Assert.Equal(before, await RegistrationCountAsync());
    }

    [Fact]
    public async Task CreateRegistration_WhenPreviousRegistrationIsCancelled_ReturnsCreated()
    {
        var eventId = await SeedEventAsync(capacity: 1);
        await SeedRegistrationAsync(eventId, "cancelled@example.com", RegistrationStatus.Cancelled);

        var response = await Client.PostAsJsonAsync($"/events/{eventId}/registrations",
            new CreateRegistrationRequest { AttendeeName = "New attendee", AttendeeEmail = "new@example.com" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal(2, await RegistrationCountAsync());
    }

    [Fact]
    public async Task CreateRegistration_WithInvalidEmail_ReturnsBadRequest()
    {
        var eventId = await SeedEventAsync();
        var response = await Client.PostAsJsonAsync($"/events/{eventId}/registrations",
            new CreateRegistrationRequest { AttendeeName = "Oksana", AttendeeEmail = "not-an-email" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetRegistrations_ReturnsOk()
    {
        var eventId = await SeedEventAsync();
        var response = await Client.GetAsync($"/events/{eventId}/registrations");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
