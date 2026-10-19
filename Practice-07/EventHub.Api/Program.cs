using EventHub.Api.Middleware;
using EventHub.Api.Options;
using EventHub.Api.Storage;
using EventHub.Application.Interfaces;
using EventHub.Application.Validation;
using EventHub.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "EventHub API",
        Version = "v1",
        Description = "API для керування подіями та їхніми обкладинками."
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

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
    var options = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<FileStorageOptions>>().Value;
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
