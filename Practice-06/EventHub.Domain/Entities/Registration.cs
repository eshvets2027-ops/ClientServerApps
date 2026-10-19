namespace EventHub.Domain.Entities;

public enum RegistrationStatus
{
    Pending,
    Cancelled
}

public sealed class Registration
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public Event? Event { get; set; }
    public string AttendeeName { get; set; } = "";
    public string AttendeeEmail { get; set; } = "";
    public RegistrationStatus Status { get; set; } = RegistrationStatus.Pending;
}
