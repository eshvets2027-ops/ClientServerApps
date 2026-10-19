using EventHub.Api.BackgroundJobs;
using EventHub.Application.BackgroundJobs;
using Xunit;

namespace EventHub.Tests;

public sealed class InMemoryJobStatusStoreTests
{
    [Fact]
    public void CreateJob_StartsPending_AndCanBeUpdated()
    {
        var store = new InMemoryJobStatusStore();
        var jobId = store.CreateJob();

        Assert.Equal(BackgroundJobStatus.Pending, store.GetStatus(jobId));

        store.SetStatus(jobId, BackgroundJobStatus.Done);

        Assert.Equal(BackgroundJobStatus.Done, store.GetStatus(jobId));
    }

    [Fact]
    public void GetStatus_ReturnsNullForUnknownJob()
    {
        var store = new InMemoryJobStatusStore();

        Assert.Null(store.GetStatus(Guid.NewGuid()));
    }
}
