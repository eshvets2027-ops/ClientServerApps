using EventHub.Api.Contracts;
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
events.MapGet("/", async (IEventRepository repo, CancellationToken ct) => Results.Ok((await repo.GetAllAsync(ct)).Select(ToEventResponse)));
events.MapGet("/{id:int}", async (int id, IEventRepository repo, CancellationToken ct) =>
{
    var item = await repo.GetByIdAsync(id, ct);
    return item is null ? Results.NotFound() : Results.Ok(ToEventResponse(item));
});
events.MapPost("/", async (CreateEventRequest request, IEventRepository repo, ICategoryRepository categoriesRepo, CancellationToken ct) =>
{
    var error = ValidateEvent(request.Title, request.Location, request.Organizer, request.Capacity, request.CategoryId);
    if (error is not null) return Results.BadRequest(new { error });
    if (await categoriesRepo.GetByIdAsync(request.CategoryId, ct) is null)
        return Results.BadRequest(new { error = "CategoryId does not exist." });
    var item = new Event { Title = request.Title.Trim(), Description = request.Description,
        Location = request.Location.Trim(), Organizer = request.Organizer.Trim(), Date = request.Date, Capacity = request.Capacity, CategoryId = request.CategoryId };
    var created = await repo.AddAsync(item, ct);
    return Results.Created($"/events/{created.Id}", ToEventResponse(created));
});
events.MapPut("/{id:int}", async (int id, UpdateEventRequest request, IEventRepository repo, ICategoryRepository categoriesRepo, CancellationToken ct) =>
{
    var error = ValidateEvent(request.Title, request.Location, request.Organizer, request.Capacity, request.CategoryId);
    if (error is not null) return Results.BadRequest(new { error });
    if (await categoriesRepo.GetByIdAsync(request.CategoryId, ct) is null)
        return Results.BadRequest(new { error = "CategoryId does not exist." });
    var item = new Event { Title = request.Title.Trim(), Description = request.Description,
        Location = request.Location.Trim(), Organizer = request.Organizer.Trim(), Date = request.Date, Capacity = request.Capacity, CategoryId = request.CategoryId };
    return await repo.UpdateAsync(id, item, ct) ? Results.NoContent() : Results.NotFound();
});
events.MapDelete("/{id:int}", async (int id, IEventRepository repo, CancellationToken ct) =>
    await repo.DeleteAsync(id, ct) ? Results.NoContent() : Results.NotFound());

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
