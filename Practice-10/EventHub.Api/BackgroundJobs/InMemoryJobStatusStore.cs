using System.Collections.Concurrent;
using EventHub.Application.BackgroundJobs;
using EventHub.Application.Interfaces;

namespace EventHub.Api.BackgroundJobs;

public sealed class InMemoryJobStatusStore : IJobStatusStore
{
    private readonly ConcurrentDictionary<Guid, BackgroundJobStatus> _statuses = new();

    public Guid CreateJob()
    {
        var jobId = Guid.NewGuid();
        _statuses[jobId] = BackgroundJobStatus.Pending;
        return jobId;
    }

    public BackgroundJobStatus? GetStatus(Guid jobId)
    {
        return _statuses.TryGetValue(jobId, out var status) ? status : null;
    }

    public void SetStatus(Guid jobId, BackgroundJobStatus status)
    {
        _statuses[jobId] = status;
    }
}
