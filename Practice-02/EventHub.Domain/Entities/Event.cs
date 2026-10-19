namespace EventHub.Domain.Entities;

public sealed class Event
{
    public int Id { get; private set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Location { get; set; } = string.Empty;
    public string Organizer { get; set; } = string.Empty;
    public DateTimeOffset Date { get; set; }
    public int Capacity { get; set; }
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
