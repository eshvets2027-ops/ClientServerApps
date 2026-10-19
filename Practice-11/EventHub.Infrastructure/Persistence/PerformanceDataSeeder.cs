using EventHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EventHub.Infrastructure.Persistence;

public sealed class PerformanceDataSeeder(AppDbContext dbContext, ILogger<PerformanceDataSeeder> logger)
{
    private const string Prefix = "Performance event ";
    private static readonly DateTime FirstEventDate = new(2030, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public async Task SeedAsync(int eventCount, int batchSize, bool reset, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(eventCount, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(batchSize, 1);

        var existing = await dbContext.Events.AnyAsync(item => item.Name.StartsWith(Prefix), cancellationToken);
        if (existing && !reset)
        {
            logger.LogInformation("Performance data already exists. Set PerformanceSeed:Reset=true to replace it.");
            return;
        }

        if (existing)
        {
            await dbContext.Events.Where(item => item.Name.StartsWith(Prefix)).ExecuteDeleteAsync(cancellationToken);
        }

        for (var offset = 0; offset < eventCount; offset += batchSize)
        {
            var count = Math.Min(batchSize, eventCount - offset);
            var events = Enumerable.Range(offset, count).Select(index => new Event
            {
                Name = $"{Prefix}{index + 1:D6}",
                Description = "Generated data for measuring EventHub database queries.",
                Location = $"Test venue {(index % 100) + 1:D3}",
                StartsAtUtc = FirstEventDate.AddMinutes(index * 53L),
                EndsAtUtc = FirstEventDate.AddMinutes(index * 53L + 60),
                Latitude = null,
                Longitude = null
            }).ToList();

            dbContext.Events.AddRange(events);
            await dbContext.SaveChangesAsync(cancellationToken);
            dbContext.ChangeTracker.Clear();
            logger.LogInformation("Seeded {SeededCount} of {EventCount} performance events.", offset + count, eventCount);
        }
    }
}
