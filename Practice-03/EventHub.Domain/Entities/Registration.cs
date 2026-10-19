namespace EventHub.Domain.Entities;

public sealed class Registration
{
    public int Id { get; private set; }
    public int EventId { get; set; }
    public Event Event { get; set; } = null!;
    public string AttendeeName { get; set; } = string.Empty;
    public string AttendeeEmail { get; set; } = string.Empty;
    public RegistrationStatus Status { get; set; } = RegistrationStatus.Pending;
    public DateTimeOffset RegisteredAt { get; set; } = DateTimeOffset.UtcNow;
}
