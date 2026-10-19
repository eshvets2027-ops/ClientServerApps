using EventHub.Application.DTOs;
using EventHub.Application.Exceptions;
using EventHub.Application.Interfaces;
using EventHub.Application.Validation;
using EventHub.Domain.Entities;
using EventHub.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventHub.Api.Controllers;

[ApiController]
[Route("events")]
public sealed class EventsController(AppDbContext dbContext, ICoverImageStorage storage, CoverImageUploadValidator validator, IGeocodingService geocodingService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEvents(
        [FromQuery] DateOnly? dateFrom,
        [FromQuery] DateOnly? dateTo,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (page < 1 || pageSize is < 1 or > 100 || (dateFrom.HasValue && dateTo.HasValue && dateFrom > dateTo))
            return BadRequest(new { error = "Invalid pagination or date range." });

        var query = dbContext.Events.AsNoTracking();

        if (dateFrom.HasValue)
        {
            var start = dateFrom.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            query = query.Where(item => item.StartsAtUtc >= start);
        }

        if (dateTo.HasValue)
        {
            var end = dateTo.Value.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            query = query.Where(item => item.StartsAtUtc < end);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(item => item.StartsAtUtc)
            .ThenBy(item => item.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(item => new EventDto(
                item.Id,
                item.Name,
                item.Description,
                item.StartsAtUtc,
                item.EndsAtUtc,
                item.CoverImagePath == null ? null : $"/events/{item.Id}/cover",
                item.Location,
                item.Latitude,
                item.Longitude,
                item.Registrations.Count))
            .ToListAsync(cancellationToken);

        return Ok(new { items, totalCount, page, pageSize });
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(EventDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventDetailDto>> GetEvent(int id)
    {
        var eventItem = await dbContext.Events.Include(item => item.Registrations).AsNoTracking().SingleOrDefaultAsync(item => item.Id == id);
        if (eventItem is null)
            throw new NotFoundException($"Event with id '{id}' does not exist.");

        return Ok(ToDetailDto(eventItem));
    }

    [HttpPost]
    [ProducesResponseType(typeof(EventDetailDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<EventDetailDto>> CreateEvent(CreateEventRequest request, CancellationToken cancellationToken)
    {
        var coordinates = await geocodingService.GeocodeAsync(request.Location, cancellationToken);
        var eventItem = new Event
        {
            Name = request.Name,
            Description = request.Description,
            StartsAtUtc = request.StartsAtUtc,
            EndsAtUtc = request.EndsAtUtc,
            Location = request.Location,
            Latitude = coordinates?.Latitude,
            Longitude = coordinates?.Longitude
        };
        dbContext.Events.Add(eventItem);
        await dbContext.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetEvent), new { id = eventItem.Id }, ToDetailDto(eventItem));
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(EventDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventDetailDto>> UpdateEvent(int id, UpdateEventRequest request, CancellationToken cancellationToken)
    {
        var eventItem = await dbContext.Events.Include(item => item.Registrations).SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (eventItem is null)
            throw new NotFoundException($"Event with id '{id}' does not exist.");

        if (!string.Equals(NormalizeLocation(eventItem.Location), NormalizeLocation(request.Location), StringComparison.OrdinalIgnoreCase))
        {
            var coordinates = await geocodingService.GeocodeAsync(request.Location, cancellationToken);
            eventItem.Latitude = coordinates?.Latitude;
            eventItem.Longitude = coordinates?.Longitude;
        }

        eventItem.Name = request.Name;
        eventItem.Description = request.Description;
        eventItem.Location = request.Location;
        eventItem.StartsAtUtc = request.StartsAtUtc;
        eventItem.EndsAtUtc = request.EndsAtUtc;

        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(ToDetailDto(eventItem));
    }

    private static string NormalizeLocation(string value) =>
        string.Join(' ', value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    [HttpPost("{id:int}/cover")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(EventDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventDetailDto>> UploadCover(int id, IFormFile? cover)
    {
        await using var content = cover?.OpenReadStream();
        var upload = cover is null ? null : new CoverImageUpload(content!, cover.FileName, cover.ContentType, cover.Length);
        var extension = validator.ValidateAndGetExtension(upload);
        var eventItem = await dbContext.Events.SingleOrDefaultAsync(item => item.Id == id);
        if (eventItem is null)
            throw new NotFoundException($"Event with id '{id}' does not exist.");

        var previousCover = eventItem.CoverImagePath;
        var newCover = await storage.SaveAsync(upload!.Content, extension);
        try
        {
            eventItem.CoverImagePath = newCover;
            await dbContext.SaveChangesAsync();
        }
        catch
        {
            await storage.DeleteAsync(newCover);
            throw;
        }

        if (!string.IsNullOrWhiteSpace(previousCover))
            await storage.DeleteAsync(previousCover);

        eventItem = await dbContext.Events.Include(item => item.Registrations).SingleAsync(item => item.Id == id);
        return Ok(ToDetailDto(eventItem));
    }

    [HttpGet("{id:int}/cover")]
    [Produces("image/jpeg", "image/png")]
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCover(int id)
    {
        var eventItem = await dbContext.Events.AsNoTracking().SingleOrDefaultAsync(item => item.Id == id);
        if (eventItem is null)
            throw new NotFoundException($"Event with id '{id}' does not exist.");
        if (string.IsNullOrWhiteSpace(eventItem.CoverImagePath))
            throw new NotFoundException($"Event with id '{id}' does not have a cover image.");

        var cover = await storage.OpenReadAsync(eventItem.CoverImagePath);
        if (cover is null)
            throw new NotFoundException($"The cover image for event with id '{id}' is unavailable.");

        return File(cover.Content, cover.ContentType, enableRangeProcessing: true);
    }

    [HttpDelete("{id:int}/cover")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCover(int id)
    {
        var eventItem = await dbContext.Events.SingleOrDefaultAsync(item => item.Id == id);
        if (eventItem is null)
            throw new NotFoundException($"Event with id '{id}' does not exist.");

        var previousCover = eventItem.CoverImagePath;
        if (string.IsNullOrWhiteSpace(previousCover))
            return NoContent();

        eventItem.CoverImagePath = null;
        await dbContext.SaveChangesAsync();
        await storage.DeleteAsync(previousCover);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        var eventItem = await dbContext.Events.SingleOrDefaultAsync(item => item.Id == id);
        if (eventItem is null)
            throw new NotFoundException($"Event with id '{id}' does not exist.");

        var previousCover = eventItem.CoverImagePath;
        dbContext.Events.Remove(eventItem);
        await dbContext.SaveChangesAsync();
        if (!string.IsNullOrWhiteSpace(previousCover))
            await storage.DeleteAsync(previousCover);
        return NoContent();
    }

    private static EventDto ToDto(Event eventItem) => new(eventItem.Id, eventItem.Name, eventItem.Description, eventItem.StartsAtUtc, eventItem.EndsAtUtc, GetCoverImageUrl(eventItem), eventItem.Location, eventItem.Latitude, eventItem.Longitude, eventItem.Registrations.Count);

    private static EventDetailDto ToDetailDto(Event eventItem) => new(eventItem.Id, eventItem.Name, eventItem.Description, eventItem.StartsAtUtc, eventItem.EndsAtUtc, GetCoverImageUrl(eventItem), eventItem.Location, eventItem.Latitude, eventItem.Longitude, eventItem.Registrations.Select(item => new RegistrationDto(item.Id, item.EventId, item.AttendeeName, item.AttendeeEmail, item.RegisteredAtUtc)).ToList());

    private static string? GetCoverImageUrl(Event eventItem) => string.IsNullOrWhiteSpace(eventItem.CoverImagePath) ? null : $"/events/{eventItem.Id}/cover";
}

public sealed class CreateEventRequest
{
    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.StringLength(200)]
    public string Name { get; init; } = string.Empty;

    [System.ComponentModel.DataAnnotations.StringLength(4000)]
    public string Description { get; init; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.StringLength(500)]
    public string Location { get; init; } = string.Empty;

    public DateTime StartsAtUtc { get; init; }
    public DateTime EndsAtUtc { get; init; }
}

public sealed class UpdateEventRequest
{
    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.StringLength(200)]
    public string Name { get; init; } = string.Empty;

    [System.ComponentModel.DataAnnotations.StringLength(4000)]
    public string Description { get; init; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.StringLength(500)]
    public string Location { get; init; } = string.Empty;

    public DateTime StartsAtUtc { get; init; }
    public DateTime EndsAtUtc { get; init; }
}
