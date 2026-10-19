using EventHub.Application.DTOs;
using EventHub.Application.Services;
using EventHub.Domain.Models;
using Microsoft.AspNetCore.Mvc;
namespace EventHub.Api.Controllers;
[ApiController]
[Route("events")]
public sealed class EventsController(EventService service) : ControllerBase
{
    [HttpGet]
    public ActionResult<IEnumerable<EventItem>> GetAll([FromQuery] EventQueryParameters query) => Ok(service.GetAll(query));
    [HttpGet("{id:int}")]
    public ActionResult<EventItem> GetById(int id) => Ok(service.GetById(id));
    [HttpPost]
    public ActionResult<EventItem> Create(CreateEventRequest request)
    {
        var item = service.Create(request);
        return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
    }
    [HttpPut("{id:int}")]
    public ActionResult<EventItem> Update(int id, UpdateEventRequest request) => Ok(service.Update(id, request));
    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id) { service.Delete(id); return NoContent(); }
    [HttpPost("{id:int}/registrations")]
    public ActionResult<EventItem> Register(int id, CreateRegistrationRequest request) => Ok(service.Register(id, request));
}
