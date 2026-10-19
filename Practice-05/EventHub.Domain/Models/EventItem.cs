namespace EventHub.Domain.Models;
public sealed class EventItem
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public required string Location { get; set; }
    public DateTimeOffset Date { get; set; }
    public int Capacity { get; set; }
    public int CategoryId { get; set; }
    public List<string> Registrations { get; set; } = [];
}
