using EventHub.Application.DTOs;

namespace EventHub.Application.Interfaces;

public interface IGeocodingService
{
    Task<Coordinates?> GeocodeAsync(string address, CancellationToken cancellationToken = default);
}
