using System.Net;
using EventHub.Application.DTOs;
using EventHub.Infrastructure.ExternalServices;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using Xunit;

namespace EventHub.Tests;

public sealed class GeoapifyGeocodingServiceIntegrationTests : IDisposable
{
    private readonly WireMockServer _server = WireMockServer.Start();
    private readonly MemoryCache _cache = new(new MemoryCacheOptions { SizeLimit = 100 });

    private GeoapifyGeocodingService CreateService()
    {
        var client = new HttpClient { BaseAddress = new Uri(_server.Url!), Timeout = TimeSpan.FromSeconds(2) };
        return new GeoapifyGeocodingService(
            client,
            _cache,
            Options.Create(new GeocodingOptions
            {
                ApiKey = "test-key",
                CacheDurationMinutes = 60,
                NegativeCacheDurationMinutes = 10
            }),
            NullLogger<GeoapifyGeocodingService>.Instance);
    }

    [Fact]
    public async Task SuccessfulResponseReturnsCoordinatesAndCachesResult()
    {
        _server.Given(Request.Create().WithPath("/v1/geocode/search").UsingGet().WithParam("text", "Kyiv, Maidan"))
            .RespondWith(Response.Create().WithStatusCode(HttpStatusCode.OK).WithHeader("Content-Type", "application/json")
                .WithBody("""{"results":[{"lat":50.4501,"lon":30.5234}]}"""));

        var service = CreateService();
        var first = await service.GeocodeAsync(" Kyiv, Maidan ");
        var second = await service.GeocodeAsync("kyiv, maidan");

        Assert.Equal(new Coordinates(50.4501, 30.5234), first);
        Assert.Equal(first, second);
        Assert.Single(_server.LogEntries);
    }

    [Fact]
    public async Task EmptyResultsReturnsNullAndCachesNegativeResult()
    {
        _server.Given(Request.Create().WithPath("/v1/geocode/search").UsingGet())
            .RespondWith(Response.Create().WithStatusCode(HttpStatusCode.OK).WithHeader("Content-Type", "application/json")
                .WithBody("""{"results":[]}"""));

        var service = CreateService();

        Assert.Null(await service.GeocodeAsync("Unknown place"));
        Assert.Null(await service.GeocodeAsync("Unknown place"));
        Assert.Single(_server.LogEntries);
    }

    [Theory]
    [InlineData(429)]
    [InlineData(503)]
    public async Task HttpFailureReturnsNullAndIsNotCached(int statusCode)
    {
        _server.Given(Request.Create().WithPath("/v1/geocode/search").UsingGet())
            .RespondWith(Response.Create().WithStatusCode(statusCode));

        var service = CreateService();

        Assert.Null(await service.GeocodeAsync("Kyiv"));
        Assert.Null(await service.GeocodeAsync("Kyiv"));
        Assert.Equal(2, _server.LogEntries.Count);
    }

    [Fact]
    public async Task InvalidJsonReturnsNullAndIsNotCached()
    {
        _server.Given(Request.Create().WithPath("/v1/geocode/search").UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("not-json"));

        var service = CreateService();

        Assert.Null(await service.GeocodeAsync("Kyiv"));
        Assert.Null(await service.GeocodeAsync("Kyiv"));
        Assert.Equal(2, _server.LogEntries.Count);
    }

    public void Dispose()
    {
        _server.Dispose();
        _cache.Dispose();
    }
}
