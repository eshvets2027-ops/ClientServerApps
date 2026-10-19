namespace EventHub.Domain.Entities;

public sealed class Event
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Location { get; set; } = "";
    public DateTimeOffset Date { get; set; }
    public int Capacity { get; set; }
    public int CategoryId { get; set; }
    public Category? Category { get; set; }
    public List<Registration> Registrations { get; set; } = [];
}
