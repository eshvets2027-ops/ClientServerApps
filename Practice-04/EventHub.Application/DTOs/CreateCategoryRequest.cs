using System.ComponentModel.DataAnnotations;
namespace EventHub.Application.DTOs;
public sealed record CreateCategoryRequest([Required, MaxLength(100)] string Name);
