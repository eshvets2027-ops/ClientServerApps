namespace EventHub.Domain.Entities;

public sealed class Registration
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public string AttendeeName { get; set; } = string.Empty;
    public string AttendeeEmail { get; set; } = string.Empty;
    public DateTime RegisteredAtUtc { get; set; }
    public Event? Event { get; set; }
}
