namespace EventHub.Application.BackgroundJobs;

public enum BackgroundJobStatus
{
    Pending,
    Processing,
    Done,
    Failed,
    Cancelled
}
