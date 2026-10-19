namespace EventHub.Api.Contracts;

public sealed record CreateEventRequest(string Title, string? Description, string Location, string Organizer, DateTimeOffset Date, int Capacity, int CategoryId);
public sealed record UpdateEventRequest(string Title, string? Description, string Location, string Organizer, DateTimeOffset Date, int Capacity, int CategoryId);
public sealed record CreateCategoryRequest(string Name);
public sealed record EventResponse(int Id, string Title, string? Description, string Location, string Organizer, DateTimeOffset Date, int Capacity, int CategoryId, DateTimeOffset CreatedAt);
public sealed record CategoryResponse(int Id, string Name);
