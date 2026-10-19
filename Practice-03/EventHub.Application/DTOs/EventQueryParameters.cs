namespace EventHub.Application.DTOs;

public sealed class EventQueryParameters
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public int? CategoryId { get; init; }
    public DateOnly? DateFrom { get; init; }
    public DateOnly? DateTo { get; init; }
    public string? Search { get; init; }
    public string? Location { get; init; }
    public string SortBy { get; init; } = "date";
    public string Order { get; init; } = "asc";
}
