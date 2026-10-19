using EventHub.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace EventHub.Api.Controllers;

/// <summary>Керування категоріями подій.</summary>
[ApiController]
[Route("categories")]
[Produces("application/json")]
public sealed class CategoriesController : ControllerBase
{
    private static readonly List<object> Categories = [];
    private static int nextId = 1;

    /// <summary>Повертає всі категорії.</summary>
    /// <response code="200">Список категорій повернуто.</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetAll() => Ok(Categories);

    /// <summary>Створює категорію.</summary>
    /// <param name="request">Назва нової категорії.</param>
    /// <response code="201">Категорію створено.</response>
    /// <response code="400">Назва не пройшла валідацію.</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Create([FromBody] CreateCategoryRequest request)
    {
        var category = new { Id = nextId++, request.Name };
        Categories.Add(category);
        return Created($"/categories/{category.Id}", category);
    }
}
