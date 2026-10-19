namespace EventHub.Application.Interfaces;

public interface IRegistrationConfirmationSender
{
    Task SendAsync(string attendeeEmail, CancellationToken cancellationToken);
}
