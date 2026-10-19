using EventHub.Api.BackgroundJobs;
using Xunit;

namespace EventHub.Tests;

public sealed class BackgroundTaskQueueTests
{
    [Fact]
    public async Task QueueAsync_DequeueAsync_ReturnsSameWorkItem()
    {
        var queue = new BackgroundTaskQueue();
        Func<CancellationToken, Task> workItem = _ => Task.CompletedTask;

        await queue.QueueAsync(workItem);
        var dequeued = await queue.DequeueAsync(CancellationToken.None);

        Assert.Same(workItem, dequeued);
    }
}
