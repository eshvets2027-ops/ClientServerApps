namespace EventHub.Domain.Entities;

public sealed class Category
{
    public int Id { get; private set; }
    public string Name { get; set; } = string.Empty;
    public ICollection<Event> Events { get; set; } = new List<Event>();
}
