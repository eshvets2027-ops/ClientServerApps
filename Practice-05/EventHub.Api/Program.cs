using System.Reflection;
using EventHub.Api.Middleware;
using EventHub.Application.Services;
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
        Description = "HTTP API для керування подіями, категоріями та реєстраціями учасників."
    });
    var apiXml = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
    var applicationXml = Path.Combine(AppContext.BaseDirectory, "EventHub.Application.xml");
    if (File.Exists(apiXml)) options.IncludeXmlComments(apiXml, true);
    if (File.Exists(applicationXml)) options.IncludeXmlComments(applicationXml, true);
});
builder.Services.AddProblemDetails(options => options.CustomizeProblemDetails = context => context.ProblemDetails.Type = null);
builder.Services.AddSingleton<EventService>();
var app = builder.Build();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseStatusCodePages();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", "EventHub API v1"));
}
app.MapControllers();
app.Run();
