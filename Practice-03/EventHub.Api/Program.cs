using EventHub.Api.Contracts;
using EventHub.Application.DTOs;
using EventHub.Domain.Entities;
using EventHub.Domain.Interfaces;
using EventHub.Infrastructure.Persistence;
using EventHub.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(connectionString);
    if (builder.Environment.IsDevelopment()) options.LogTo(Console.WriteLine, LogLevel.Information);
});
builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => Results.Ok(new { name = "EventHub API", docs = "/swagger" }));

var categories = app.MapGroup("/categories").WithTags("Categories");
categories.MapGet("/", async (ICategoryRepository repo, CancellationToken ct) =>
    Results.Ok((await repo.GetAllAsync(ct)).Select(ToCategoryResponse)));
categories.MapGet("/{id:int}", async (int id, ICategoryRepository repo, CancellationToken ct) =>
{
    var category = await repo.GetByIdAsync(id, ct);
    return category is null ? Results.NotFound() : Results.Ok(ToCategoryResponse(category));
});
categories.MapPost("/", async (CreateCategoryRequest request, ICategoryRepository repo, CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Length > 100)
        return Results.BadRequest(new { error = "Name is required and must be at most 100 characters." });
    try
    {
        var created = await repo.AddAsync(new Category { Name = request.Name.Trim() }, ct);
        return Results.Created($"/categories/{created.Id}", ToCategoryResponse(created));
    }
    catch (DbUpdateException)
    {
        return Results.Conflict(new { error = "A category with this name may already exist." });
    }
});

var events = app.MapGroup("/events").WithTags("Events");
events.MapGet("/", async ([AsParameters] EventQueryParameters parameters, AppDbContext db, CancellationToken ct) =>
{
    if (parameters.Page < 1)
        return Results.BadRequest(new { error = "Page must be greater than or equal to 1." });
    if (parameters.PageSize is < 1 or > 100)
        return Results.BadRequest(new { error = "PageSize must be between 1 and 100." });
    if (parameters.CategoryId is <= 0)
        return Results.BadRequest(new { error = "CategoryId must be greater than zero." });
    if (parameters.DateFrom.HasValue && parameters.DateTo.HasValue && parameters.DateFrom.Value > parameters.DateTo.Value)
        return Results.BadRequest(new { error = "DateFrom cannot be later than DateTo." });
    if (!string.IsNullOrWhiteSpace(parameters.Search) && parameters.Search.Trim().Length > 100)
        return Results.BadRequest(new { error = "Search must be at most 100 characters." });
    if (!string.IsNullOrWhiteSpace(parameters.Location) && parameters.Location.Trim().Length > 200)
        return Results.BadRequest(new { error = "Location must be at most 200 characters." });

    IQueryable<Event> query = db.Events.AsNoTracking();
    if (parameters.CategoryId.HasValue)
        query = query.Where(x => x.CategoryId == parameters.CategoryId.Value);
    if (!string.IsNullOrWhiteSpace(parameters.Search))
    {
        var search = parameters.Search.Trim();
        query = query.Where(x => EF.Functions.ILike(x.Title, $"%{search}%"));
    }
    if (!string.IsNullOrWhiteSpace(parameters.Location))
    {
        var location = parameters.Location.Trim();
        query = query.Where(x => EF.Functions.ILike(x.Location, $"%{location}%"));
    }
    if (parameters.DateFrom.HasValue)
    {
        var from = new DateTimeOffset(parameters.DateFrom.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
        query = query.Where(x => x.Date >= from);
    }
    if (parameters.DateTo.HasValue)
    {
        var exclusiveTo = new DateTimeOffset(parameters.DateTo.Value.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
        query = query.Where(x => x.Date < exclusiveTo);
    }

    var sortBy = (parameters.SortBy ?? "date").Trim().ToLowerInvariant();
    var order = (parameters.Order ?? "asc").Trim().ToLowerInvariant();
    if (sortBy is not ("title" or "date" or "location") || order is not ("asc" or "desc"))
        return Results.BadRequest(new { error = "SortBy must be title, date or location; Order must be asc or desc." });

    query = (sortBy, order) switch
    {
        ("title", "asc") => query.OrderBy(x => x.Title).ThenBy(x => x.Id),
        ("title", "desc") => query.OrderByDescending(x => x.Title).ThenBy(x => x.Id),
        ("location", "asc") => query.OrderBy(x => x.Location).ThenBy(x => x.Id),
        ("location", "desc") => query.OrderByDescending(x => x.Location).ThenBy(x => x.Id),
        ("date", "desc") => query.OrderByDescending(x => x.Date).ThenBy(x => x.Id),
        _ => query.OrderBy(x => x.Date).ThenBy(x => x.Id)
    };

    var totalCount = await query.CountAsync(ct);
    var items = await query
        .Skip((parameters.Page - 1) * parameters.PageSize)
        .Take(parameters.PageSize)
        .Select(x => new EventDto(
            x.Id,
            x.Title,
            x.Description,
            x.Location,
            x.Date,
            x.Capacity,
            x.CategoryId,
            x.Category.Name,
            x.Registrations.Count,
            x.CreatedAt))
        .ToListAsync(ct);

    return Results.Ok(new PagedResult<EventDto>(items, totalCount, parameters.Page, parameters.PageSize));
});

events.MapGet("/{id:int}", async (int id, AppDbContext db, CancellationToken ct) =>
{
    var detail = await db.Events
        .AsNoTracking()
        .Where(x => x.Id == id)
        .Select(x => new EventDetailDto(
            x.Id,
            x.Title,
            x.Description,
            x.Location,
            x.Date,
            x.Capacity,
            x.CategoryId,
            x.Category.Name,
            x.CreatedAt,
            x.Registrations
                .OrderByDescending(r => r.RegisteredAt)
                .ThenByDescending(r => r.Id)
                .Select(r => new RegistrationDto(r.Id, r.EventId, r.AttendeeName, r.AttendeeEmail, r.Status.ToString(), r.RegisteredAt))
                .ToList()))
        .FirstOrDefaultAsync(ct);
    return detail is null ? Results.NotFound() : Results.Ok(detail);
});

events.MapPost("/", async (CreateEventRequest request, IEventRepository repo, ICategoryRepository categoriesRepo, CancellationToken ct) =>
{
    var error = ValidateEvent(request.Title, request.Location, request.Organizer, request.Capacity, request.CategoryId);
    if (error is not null) return Results.BadRequest(new { error });
    if (await categoriesRepo.GetByIdAsync(request.CategoryId, ct) is null)
        return Results.BadRequest(new { error = "CategoryId does not exist." });
    var item = new Event
    {
        Title = request.Title.Trim(), Description = request.Description,
        Location = request.Location.Trim(), Organizer = request.Organizer.Trim(),
        Date = request.Date, Capacity = request.Capacity, CategoryId = request.CategoryId
    };
    var created = await repo.AddAsync(item, ct);
    return Results.Created($"/events/{created.Id}", ToEventResponse(created));
});

events.MapPut("/{id:int}", async (int id, UpdateEventRequest request, IEventRepository repo, ICategoryRepository categoriesRepo, CancellationToken ct) =>
{
    var error = ValidateEvent(request.Title, request.Location, request.Organizer, request.Capacity, request.CategoryId);
    if (error is not null) return Results.BadRequest(new { error });
    if (await categoriesRepo.GetByIdAsync(request.CategoryId, ct) is null)
        return Results.BadRequest(new { error = "CategoryId does not exist." });
    var item = new Event
    {
        Title = request.Title.Trim(), Description = request.Description,
        Location = request.Location.Trim(), Organizer = request.Organizer.Trim(),
        Date = request.Date, Capacity = request.Capacity, CategoryId = request.CategoryId
    };
    return await repo.UpdateAsync(id, item, ct) ? Results.NoContent() : Results.NotFound();
});

events.MapDelete("/{id:int}", async (int id, IEventRepository repo, CancellationToken ct) =>
    await repo.DeleteAsync(id, ct) ? Results.NoContent() : Results.NotFound());

events.MapPost("/{eventId:int}/registrations", async (int eventId, CreateRegistrationRequest request, AppDbContext db, CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(request.AttendeeName) || request.AttendeeName.Trim().Length > 100)
        return Results.BadRequest(new { error = "AttendeeName is required and must be at most 100 characters." });
    if (string.IsNullOrWhiteSpace(request.AttendeeEmail) || request.AttendeeEmail.Trim().Length > 320 || !request.AttendeeEmail.Contains('@'))
        return Results.BadRequest(new { error = "A valid AttendeeEmail of at most 320 characters is required." });

    var eventItem = await db.Events.AsNoTracking().Where(x => x.Id == eventId)
        .Select(x => new { x.Id, x.Capacity, RegistrationCount = x.Registrations.Count(r => r.Status != RegistrationStatus.Cancelled) })
        .FirstOrDefaultAsync(ct);
    if (eventItem is null) return Results.NotFound(new { error = "Event does not exist." });
    if (eventItem.RegistrationCount >= eventItem.Capacity)
        return Results.Conflict(new { error = "The event has reached its registration capacity." });

    var registration = new Registration
    {
        EventId = eventId,
        AttendeeName = request.AttendeeName.Trim(),
        AttendeeEmail = request.AttendeeEmail.Trim(),
        Status = RegistrationStatus.Pending,
        RegisteredAt = DateTimeOffset.UtcNow
    };
    db.Registrations.Add(registration);
    await db.SaveChangesAsync(ct);
    var response = new RegistrationDto(registration.Id, registration.EventId, registration.AttendeeName,
        registration.AttendeeEmail, registration.Status.ToString(), registration.RegisteredAt);
    return Results.Created($"/events/{eventId}/registrations/{registration.Id}", response);
});

events.MapGet("/{eventId:int}/registrations", async (int eventId, AppDbContext db, CancellationToken ct) =>
{
    var eventExists = await db.Events.AnyAsync(x => x.Id == eventId, ct);
    if (!eventExists) return Results.NotFound(new { error = "Event does not exist." });
    var registrations = await db.Registrations.AsNoTracking()
        .Where(x => x.EventId == eventId)
        .OrderByDescending(x => x.RegisteredAt).ThenByDescending(x => x.Id)
        .Select(x => new RegistrationDto(x.Id, x.EventId, x.AttendeeName, x.AttendeeEmail, x.Status.ToString(), x.RegisteredAt))
        .ToListAsync(ct);
    return Results.Ok(registrations);
});

app.Run();

static EventResponse ToEventResponse(Event item) => new(item.Id, item.Title, item.Description, item.Location, item.Organizer, item.Date, item.Capacity, item.CategoryId, item.CreatedAt);

static CategoryResponse ToCategoryResponse(Category item) => new(item.Id, item.Name);

static string? ValidateEvent(string title, string location, string organizer, int capacity, int categoryId)
{
    if (string.IsNullOrWhiteSpace(title) || title.Length > 200) return "Title is required and must be at most 200 characters.";
    if (string.IsNullOrWhiteSpace(location) || location.Length > 500) return "Location is required and must be at most 500 characters.";
    if (string.IsNullOrWhiteSpace(organizer) || organizer.Length > 120) return "Organizer is required and must be at most 120 characters.";
    if (capacity < 1) return "Capacity must be greater than zero.";
    if (categoryId < 1) return "CategoryId must be a positive number.";
    return null;
}
