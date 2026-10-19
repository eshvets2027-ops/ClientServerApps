using EventHub.Api.Middleware;
using EventHub.Api.Options;
using EventHub.Api.Storage;
using EventHub.Application.Interfaces;
using EventHub.Application.Validation;
using EventHub.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSingleton<IValidateOptions<OpenApiOptions>, OpenApiOptionsValidator>();
builder.Services.AddOptions<OpenApiOptions>()
    .Bind(builder.Configuration.GetSection(OpenApiOptions.SectionName))
    .ValidateOnStart();
builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();
builder.Services.AddSwaggerGen();

builder.Services.AddOptions<DatabaseOptions>()
    .Configure<IConfiguration>((options, configuration) =>
        options.ConnectionString = configuration.GetConnectionString("DefaultConnection") ?? string.Empty)
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.ConnectionString),
        "ConnectionStrings:DefaultConnection is required. Use .NET user secrets locally or an environment variable in deployed environments.")
    .ValidateOnStart();

builder.Services.AddDbContext<AppDbContext>((serviceProvider, options) =>
{
    var databaseOptions = serviceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
    var environment = serviceProvider.GetRequiredService<IHostEnvironment>();
    options.UseNpgsql(databaseOptions.ConnectionString);
    if (environment.IsDevelopment())
    {
        options.EnableDetailedErrors();
        options.LogTo(
            Console.WriteLine,
            [DbLoggerCategory.Database.Command.Name],
            LogLevel.Information);
    }
});

builder.Services.AddOptions<FileStorageOptions>()
    .Bind(builder.Configuration.GetSection(FileStorageOptions.SectionName))
    .Validate(options => !string.IsNullOrWhiteSpace(options.UploadPath), "FileStorage:UploadPath is required.")
    .Validate(options => options.MaxFileSizeMb is > 0 and <= 100, "FileStorage:MaxFileSizeMb must be between 1 and 100.")
    .ValidateOnStart();

builder.Services.Configure<FormOptions>(options =>
{
    var maxFileSizeMb = builder.Configuration.GetValue<int?>($"{FileStorageOptions.SectionName}:MaxFileSizeMb") ?? 5;
    options.MultipartBodyLengthLimit = maxFileSizeMb * 1024L * 1024L;
});

builder.Services.AddSingleton<ICoverImageStorage, LocalCoverImageStorage>();
builder.Services.AddSingleton(serviceProvider =>
{
    var options = serviceProvider.GetRequiredService<IOptions<FileStorageOptions>>().Value;
    return new CoverImageUploadValidator(options.MaxFileSizeMb);
});

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();
app.MapControllers();
app.Run();

public partial class Program { }
