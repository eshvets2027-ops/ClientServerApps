using EventHub.Application.DTOs;
using EventHub.Application.Services;
using EventHub.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace EventHub.Api.Controllers;

/// <summary>Керування подіями та вкладеними реєстраціями учасників.</summary>
[ApiController]
[Route("events")]
[Produces("application/json")]
public sealed class EventsController(EventService service) : ControllerBase
{
    /// <summary>Повертає сторінку подій із фільтрами.</summary>
    /// <param name="query">Параметри пошуку, дат і пагінації.</param>
    /// <response code="200">Список подій успішно повернуто.</response>
    /// <response code="400">Параметри запиту не пройшли валідацію.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<EventItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<IEnumerable<EventItem>> GetAll([FromQuery] EventQueryParameters query) => Ok(service.GetAll(query));

    /// <summary>Повертає подію за ідентифікатором.</summary>
    /// <param name="id">Ідентифікатор події.</param>
    /// <response code="200">Подію знайдено.</response>
    /// <response code="404">Подію не знайдено.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(EventItem), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<EventItem> GetById([FromRoute] int id) => Ok(service.GetById(id));

    /// <summary>Створює подію.</summary>
    /// <param name="request">Дані нової події.</param>
    /// <response code="201">Подію створено.</response>
    /// <response code="400">Дані не пройшли валідацію.</response>
    [HttpPost]
    [ProducesResponseType(typeof(EventItem), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<EventItem> Create([FromBody] CreateEventRequest request)
    {
        var item = service.Create(request);
        return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
    }

    /// <summary>Оновлює наявну подію.</summary>
    /// <param name="id">Ідентифікатор події.</param>
    /// <param name="request">Оновлені дані події.</param>
    /// <response code="200">Подію оновлено.</response>
    /// <response code="400">Дані не пройшли валідацію.</response>
    /// <response code="404">Подію не знайдено.</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(EventItem), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<EventItem> Update([FromRoute] int id, [FromBody] UpdateEventRequest request) => Ok(service.Update(id, request));

    /// <summary>Видаляє подію.</summary>
    /// <param name="id">Ідентифікатор події.</param>
    /// <response code="204">Подію видалено.</response>
    /// <response code="404">Подію не знайдено.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete([FromRoute] int id) { service.Delete(id); return NoContent(); }

    /// <summary>Повертає реєстрації учасників події.</summary>
    /// <param name="id">Ідентифікатор події.</param>
    /// <response code="200">Список реєстрацій повернуто.</response>
    /// <response code="404">Подію не знайдено.</response>
    [HttpGet("{id:int}/registrations")]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<IEnumerable<string>> GetRegistrations([FromRoute] int id) => Ok(service.GetRegistrations(id));

    /// <summary>Реєструє учасника на подію.</summary>
    /// <param name="id">Ідентифікатор події.</param>
    /// <param name="request">Email учасника.</param>
    /// <response code="201">Учасника зареєстровано.</response>
    /// <response code="400">Дані не пройшли валідацію або реєстрація недоступна.</response>
    /// <response code="404">Подію не знайдено.</response>
    /// <response code="409">Учасник уже зареєстрований.</response>
    [HttpPost("{id:int}/registrations")]
    [ProducesResponseType(typeof(EventItem), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public ActionResult<EventItem> Register([FromRoute] int id, [FromBody] CreateRegistrationRequest request)
    {
        var item = service.Register(id, request);
        return StatusCode(StatusCodes.Status201Created, item);
    }

    /// <summary>Скасовує реєстрацію учасника на подію.</summary>
    /// <param name="id">Ідентифікатор події.</param>
    /// <param name="email">Email зареєстрованого учасника.</param>
    /// <response code="204">Реєстрацію скасовано.</response>
    /// <response code="404">Подію або реєстрацію не знайдено.</response>
    [HttpDelete("{id:int}/registrations")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult CancelRegistration([FromRoute] int id, [FromQuery] string email)
    {
        service.CancelRegistration(id, email);
        return NoContent();
    }
}
