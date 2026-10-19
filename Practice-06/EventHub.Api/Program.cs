using EventHub.Application.Services;
using EventHub.Domain.Interfaces;
using EventHub.Infrastructure.Persistence;
using EventHub.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "EventHub API",
        Version = "v1",
        Description = "API для подій та реєстрацій учасників."
    });
    var xmlPath = Path.Combine(AppContext.BaseDirectory, "EventHub.Api.xml");
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath, true);
});

builder.Services.AddDbContext<AppDbContext>((serviceProvider, options) =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("DefaultConnection")
        ?? "Host=localhost;Port=5432;Database=eventhub;Username=postgres;Password=postgres";
    options.UseNpgsql(connectionString);
});
builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<IRegistrationRepository, RegistrationRepository>();
builder.Services.AddScoped<RegistrationService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();

public partial class Program;
