using EventHub.Application.DTOs;
using EventHub.Application.Services;
using EventHub.Domain.Entities;
using EventHub.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventHub.Api.Controllers;

[ApiController]
[Route("events/{eventId:int}/registrations")]
[Produces("application/json")]
public sealed class RegistrationsController(RegistrationService service, AppDbContext db) : ControllerBase
{
    /// <summary>Створює реєстрацію учасника на подію.</summary>
    /// <param name="eventId">Ідентифікатор події.</param>
    /// <param name="request">Дані учасника.</param>
    /// <returns>Створена реєстрація.</returns>
    /// <response code="201">Реєстрацію створено.</response>
    /// <response code="400">Дані запиту не пройшли валідацію.</response>
    /// <response code="404">Подію не знайдено.</response>
    /// <response code="409">Email уже зареєстрований на цю подію.</response>
    /// <response code="422">Вільних місць немає.</response>
    [HttpPost]
    [ProducesResponseType(typeof(RegistrationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<RegistrationDto>> Create(int eventId, CreateRegistrationRequest request)
    {
        try
        {
            var result = await service.CreateAsync(eventId, request);
            return Created($"/events/{eventId}/registrations", result);
        }
        catch (RegistrationNotFoundException)
        {
            return Problem(statusCode: 404, title: "Event not found", detail: "The event does not exist.");
        }
        catch (RegistrationConflictException)
        {
            return Problem(statusCode: 409, title: "Resource conflict", detail: "This email is already registered.");
        }
        catch (RegistrationCapacityException)
        {
            return Problem(statusCode: 422, title: "Capacity reached", detail: "No seats are available.");
        }
    }

    /// <summary>Повертає реєстрації для вказаної події.</summary>
    /// <param name="eventId">Ідентифікатор події.</param>
    /// <returns>Список реєстрацій.</returns>
    /// <response code="200">Список повернуто.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<RegistrationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<RegistrationDto>>> GetAll(int eventId)
    {
        var items = await db.Registrations.AsNoTracking()
            .Where(x => x.EventId == eventId)
            .OrderBy(x => x.Id)
            .Select(x => new RegistrationDto(x.Id, x.EventId, x.AttendeeName, x.AttendeeEmail,
                x.Status == RegistrationStatus.Cancelled ? "cancelled" : "pending"))
            .ToListAsync();
        return Ok(items);
    }
}
