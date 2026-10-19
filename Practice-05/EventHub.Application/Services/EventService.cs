using EventHub.Application.DTOs;
using EventHub.Domain.Exceptions;
using EventHub.Domain.Models;
namespace EventHub.Application.Services;
public sealed class EventService
{
    private readonly List<EventItem> events = [];
    private int nextId = 1;
    public IEnumerable<EventItem> GetAll(EventQueryParameters query)
    {
        IEnumerable<EventItem> result = events;
        if (!string.IsNullOrWhiteSpace(query.Search)) result = result.Where(x => x.Title.Contains(query.Search, StringComparison.OrdinalIgnoreCase));
        if (query.DateFrom.HasValue) result = result.Where(x => DateOnly.FromDateTime(x.Date.UtcDateTime) >= query.DateFrom.Value);
        if (query.DateTo.HasValue) result = result.Where(x => DateOnly.FromDateTime(x.Date.UtcDateTime) <= query.DateTo.Value);
        return result.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).Select(Clone).ToList();
    }
    public EventItem GetById(int id) => Clone(events.FirstOrDefault(x => x.Id == id) ?? throw new NotFoundException($"Event with id '{id}' does not exist."));
    public EventItem Create(CreateEventRequest request)
    {
        var item = new EventItem { Id = nextId++, Title = request.Title, Description = request.Description, Location = request.Location, Date = request.Date, Capacity = request.Capacity, CategoryId = request.CategoryId };
        events.Add(item);
        return Clone(item);
    }
    public EventItem Update(int id, UpdateEventRequest request)
    {
        var item = events.FirstOrDefault(x => x.Id == id) ?? throw new NotFoundException($"Event with id '{id}' does not exist.");
        item.Title = request.Title; item.Description = request.Description; item.Location = request.Location; item.Date = request.Date; item.Capacity = request.Capacity; item.CategoryId = request.CategoryId;
        return Clone(item);
    }
    public void Delete(int id)
    {
        var item = events.FirstOrDefault(x => x.Id == id) ?? throw new NotFoundException($"Event with id '{id}' does not exist.");
        events.Remove(item);
    }
    public EventItem Register(int id, CreateRegistrationRequest request)
    {
        var item = events.FirstOrDefault(x => x.Id == id) ?? throw new NotFoundException($"Event with id '{id}' does not exist.");
        if (item.Date <= DateTimeOffset.UtcNow) throw new BusinessRuleValidationException("Registration is not allowed for past events.");
        var email = request.AttendeeEmail.Trim().ToLowerInvariant();
        if (item.Registrations.Contains(email, StringComparer.OrdinalIgnoreCase)) throw new ConflictException("This attendee is already registered for the event.");
        if (item.Registrations.Count >= item.Capacity) throw new BusinessRuleValidationException("The event is at full capacity.");
        item.Registrations.Add(email);
        return Clone(item);
    }
    public IEnumerable<string> GetRegistrations(int id)
    {
        var item = events.FirstOrDefault(x => x.Id == id) ?? throw new NotFoundException($"Event with id '{id}' does not exist.");
        return item.Registrations.ToList();
    }
    public void CancelRegistration(int id, string email)
    {
        var item = events.FirstOrDefault(x => x.Id == id) ?? throw new NotFoundException($"Event with id '{id}' does not exist.");
        var existing = item.Registrations.FirstOrDefault(x => string.Equals(x, email.Trim(), StringComparison.OrdinalIgnoreCase));
        if (existing is null) throw new NotFoundException("Registration does not exist.");
        item.Registrations.Remove(existing);
    }
    private static EventItem Clone(EventItem item) => new() { Id = item.Id, Title = item.Title, Description = item.Description, Location = item.Location, Date = item.Date, Capacity = item.Capacity, CategoryId = item.CategoryId, Registrations = [.. item.Registrations] };
}
