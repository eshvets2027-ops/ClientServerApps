using EventHub.Application.DTOs;
using Microsoft.AspNetCore.Mvc;
namespace EventHub.Api.Controllers;
[ApiController]
[Route("categories")]
public sealed class CategoriesController : ControllerBase
{
    private static readonly List<object> Categories = [];
    private static int nextId = 1;
    [HttpGet]
    public IActionResult GetAll() => Ok(Categories);
    [HttpPost]
    public IActionResult Create(CreateCategoryRequest request)
    {
        var category = new { Id = nextId++, request.Name };
        Categories.Add(category);
        return Created($"/categories/{category.Id}", category);
    }
}
