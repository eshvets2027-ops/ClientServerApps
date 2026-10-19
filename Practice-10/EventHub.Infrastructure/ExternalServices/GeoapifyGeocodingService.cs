using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using EventHub.Application.DTOs;
using EventHub.Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EventHub.Infrastructure.ExternalServices;

public sealed class GeoapifyGeocodingService(
    HttpClient httpClient,
    IMemoryCache cache,
    IOptions<GeocodingOptions> options,
    ILogger<GeoapifyGeocodingService> logger) : IGeocodingService
{
    private readonly GeocodingOptions _options = options.Value;

    public async Task<Coordinates?> GeocodeAsync(string address, CancellationToken cancellationToken = default)
    {
        var normalizedAddress = CollapseWhitespace(address);
        if (string.IsNullOrWhiteSpace(normalizedAddress))
            return null;

        var cacheKey = $"geocoding:{normalizedAddress.ToUpperInvariant()}";
        if (cache.TryGetValue(cacheKey, out CacheEntry? cached))
            return cached?.Coordinates;

        var lookup = await RequestCoordinatesAsync(normalizedAddress, cancellationToken);
        if (lookup.ShouldCache)
        {
            cache.Set(
                cacheKey,
                new CacheEntry(lookup.Coordinates),
                new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(
                        lookup.Coordinates is null ? _options.NegativeCacheDurationMinutes : _options.CacheDurationMinutes),
                    Size = 1
                });
        }

        return lookup.Coordinates;
    }

    private async Task<GeocodingLookup> RequestCoordinatesAsync(string address, CancellationToken cancellationToken)
    {
        try
        {
            using var response = await httpClient.GetAsync(
                BuildRequestUri(address),
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Geoapify failed with HTTP status {StatusCode}.", (int)response.StatusCode);
                return GeocodingLookup.TransientFailure;
            }

            var payload = await response.Content.ReadFromJsonAsync<GeoapifyResponse>(cancellationToken: cancellationToken);
            var result = payload?.Results.FirstOrDefault();
            if (result is null)
                return GeocodingLookup.Cacheable(null);

            if (result.Latitude is < -90 or > 90 || result.Longitude is < -180 or > 180)
                return GeocodingLookup.TransientFailure;

            return GeocodingLookup.Cacheable(new Coordinates(result.Latitude, result.Longitude));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Geoapify geocoding timed out.");
            return GeocodingLookup.TransientFailure;
        }
        catch (HttpRequestException exception)
        {
            logger.LogWarning(exception, "Geoapify geocoding is unavailable.");
            return GeocodingLookup.TransientFailure;
        }
        catch (JsonException exception)
        {
            logger.LogWarning(exception, "Geoapify returned invalid JSON.");
            return GeocodingLookup.TransientFailure;
        }
    }

    private string BuildRequestUri(string address) =>
        "v1/geocode/search" +
        $"?text={Uri.EscapeDataString(address)}" +
        "&format=json&limit=1" +
        $"&lang={Uri.EscapeDataString(_options.Language)}" +
        $"&filter={Uri.EscapeDataString($"countrycode:{_options.CountryCode}")}" +
        $"&apiKey={Uri.EscapeDataString(_options.ApiKey)}";

    private static string CollapseWhitespace(string value) =>
        string.Join(' ', value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    private sealed record CacheEntry(Coordinates? Coordinates);
    private sealed record GeocodingLookup(Coordinates? Coordinates, bool ShouldCache)
    {
        public static GeocodingLookup TransientFailure { get; } = new(null, false);
        public static GeocodingLookup Cacheable(Coordinates? coordinates) => new(coordinates, true);
    }

    private sealed class GeoapifyResponse
    {
        [JsonPropertyName("results")]
        public IReadOnlyList<GeoapifyResult> Results { get; init; } = [];
    }

    private sealed class GeoapifyResult
    {
        [JsonPropertyName("lat")]
        public double Latitude { get; init; }

        [JsonPropertyName("lon")]
        public double Longitude { get; init; }
    }
}
