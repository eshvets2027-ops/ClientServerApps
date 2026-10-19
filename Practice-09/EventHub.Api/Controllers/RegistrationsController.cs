using EventHub.Application.BackgroundJobs;
using EventHub.Application.DTOs;
using EventHub.Application.Exceptions;
using EventHub.Application.Interfaces;
using EventHub.Domain.Entities;
using EventHub.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventHub.Api.Controllers;

[ApiController]
[Route("events/{eventId:int}/registrations")]
public sealed class RegistrationsController(
    AppDbContext dbContext,
    IBackgroundTaskQueue taskQueue,
    IJobStatusStore jobStatusStore,
    IRegistrationConfirmationSender confirmationSender) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(CreateRegistrationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CreateRegistrationResponse>> Create(
        int eventId,
        CreateRegistrationRequest request,
        CancellationToken cancellationToken)
    {
        var eventExists = await dbContext.Events.AnyAsync(item => item.Id == eventId, cancellationToken);
        if (!eventExists)
            throw new NotFoundException($"Event with id '{eventId}' does not exist.");

        var email = request.AttendeeEmail.Trim();
        var duplicate = await dbContext.Registrations.AnyAsync(
            item => item.EventId == eventId && item.AttendeeEmail == email,
            cancellationToken);
        if (duplicate)
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Registration already exists",
                Detail = "This email is already registered for the event."
            });

        var registration = new Registration
        {
            EventId = eventId,
            AttendeeName = request.AttendeeName.Trim(),
            AttendeeEmail = email,
            RegisteredAtUtc = DateTime.UtcNow
        };

        dbContext.Registrations.Add(registration);
        await dbContext.SaveChangesAsync(cancellationToken);

        var jobId = jobStatusStore.CreateJob();
        var statusStore = jobStatusStore;
        var sender = confirmationSender;
        await taskQueue.QueueAsync(token => ProcessConfirmationJobAsync(
            jobId, email, statusStore, sender, token));

        var response = new CreateRegistrationResponse(
            new RegistrationDto(registration.Id, registration.EventId, registration.AttendeeName,
                registration.AttendeeEmail, registration.RegisteredAtUtc),
            jobId);

        return StatusCode(StatusCodes.Status201Created, response);
    }

    private static async Task ProcessConfirmationJobAsync(
        Guid jobId,
        string attendeeEmail,
        IJobStatusStore statusStore,
        IRegistrationConfirmationSender sender,
        CancellationToken cancellationToken)
    {
        statusStore.SetStatus(jobId, BackgroundJobStatus.Processing);
        try
        {
            await sender.SendAsync(attendeeEmail, cancellationToken);
            statusStore.SetStatus(jobId, BackgroundJobStatus.Done);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            statusStore.SetStatus(jobId, BackgroundJobStatus.Cancelled);
            throw;
        }
        catch
        {
            statusStore.SetStatus(jobId, BackgroundJobStatus.Failed);
            throw;
        }
    }
}

public sealed class CreateRegistrationRequest
{
    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.StringLength(200)]
    public string AttendeeName { get; init; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.EmailAddress]
    [System.ComponentModel.DataAnnotations.StringLength(320)]
    public string AttendeeEmail { get; init; } = string.Empty;
}
