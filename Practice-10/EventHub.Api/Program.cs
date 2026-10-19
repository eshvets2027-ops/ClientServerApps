using EventHub.Api.Middleware;
using EventHub.Api.BackgroundJobs;
using EventHub.Api.Options;
using EventHub.Api.Storage;
using EventHub.Application.Interfaces;
using EventHub.Application.Validation;
using EventHub.Infrastructure.Persistence;
using EventHub.Infrastructure.ExternalServices;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddOptions<GeocodingOptions>()
    .Bind(builder.Configuration.GetSection(GeocodingOptions.SectionName))
    .Validate(options => Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out var baseUrl) && baseUrl.Scheme == Uri.UriSchemeHttps, "Geocoding:BaseUrl must be an absolute HTTPS URL.")
    .Validate(options => !string.IsNullOrWhiteSpace(options.ApiKey), "Geocoding:ApiKey is required.")
    .Validate(options => options.TimeoutSeconds is > 0 and <= 60, "Geocoding:TimeoutSeconds must be between 1 and 60.")
    .Validate(options => options.CacheDurationMinutes > 0, "Geocoding:CacheDurationMinutes must be positive.")
    .Validate(options => options.NegativeCacheDurationMinutes > 0, "Geocoding:NegativeCacheDurationMinutes must be positive.")
    .Validate(options => options.CacheSize > 0, "Geocoding:CacheSize must be positive.")
    .ValidateOnStart();

builder.Services.AddMemoryCache(options =>
{
    options.SizeLimit = builder.Configuration.GetValue<long?>("Geocoding:CacheSize") ?? 1_000;
});

builder.Services.AddHttpClient<IGeocodingService, GeoapifyGeocodingService>((serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<IOptions<GeocodingOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
    client.DefaultRequestHeaders.UserAgent.ParseAdd("EventHub/1.0");
});

builder.Services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
builder.Services.AddSingleton<IJobStatusStore, InMemoryJobStatusStore>();
builder.Services.AddSingleton<IRegistrationConfirmationSender, LoggingRegistrationConfirmationSender>();
builder.Services.AddHostedService<BackgroundTaskWorker>();
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
