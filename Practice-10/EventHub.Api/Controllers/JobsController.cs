using EventHub.Application.DTOs;
using EventHub.Application.Exceptions;
using EventHub.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EventHub.Api.Controllers;

[ApiController]
[Route("jobs")]
public sealed class JobsController(IJobStatusStore jobStatusStore) : ControllerBase
{
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(JobStatusDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public ActionResult<JobStatusDto> GetById(Guid id)
    {
        var status = jobStatusStore.GetStatus(id);
        if (status is null)
            throw new NotFoundException($"Background job with id '{id}' does not exist.");

        return Ok(new JobStatusDto(id, status.Value.ToString().ToLowerInvariant()));
    }
}
