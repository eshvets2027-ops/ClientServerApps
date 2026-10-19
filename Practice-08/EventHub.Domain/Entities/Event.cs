namespace EventHub.Domain.Entities;

public sealed class Event
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartsAtUtc { get; set; }
    public DateTime EndsAtUtc { get; set; }
    public string? CoverImagePath { get; set; }
    public ICollection<Registration> Registrations { get; set; } = new List<Registration>();
}
