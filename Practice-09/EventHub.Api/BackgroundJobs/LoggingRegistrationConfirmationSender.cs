using EventHub.Application.Interfaces;

namespace EventHub.Api.BackgroundJobs;

public sealed class LoggingRegistrationConfirmationSender(
    ILogger<LoggingRegistrationConfirmationSender> logger) : IRegistrationConfirmationSender
{
    public async Task SendAsync(string attendeeEmail, CancellationToken cancellationToken)
    {
        logger.LogInformation("Sending registration confirmation to {AttendeeEmail}...", attendeeEmail);
        await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
        logger.LogInformation("Registration confirmation sent to {AttendeeEmail}.", attendeeEmail);
    }
}
