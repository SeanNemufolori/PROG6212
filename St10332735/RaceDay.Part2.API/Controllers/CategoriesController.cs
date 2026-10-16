using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Filters;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly AppDbContext _db;

    public CategoriesController(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// View categories for an event (public - both roles).
    /// </summary>
    [HttpGet("event/{eventId}")]
    public IActionResult GetCategoriesForEvent(int eventId)
    {
        var categories = _db.Categories
            .Where(c => c.EventId == eventId)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                EventId = c.EventId,
                EventName = c.Event.Name,
                Name = c.Name,
                Description = c.Description
            })
            .ToList();

        return Ok(categories);
    }

    /// <summary>
    /// Create a category for an event (Organiser only - must own the event).
    /// </summary>
    [HttpPost]
    [OrganiserOnly]
    public IActionResult CreateCategory([FromBody] CreateCategoryDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { message = "Invalid category data.", errors = ModelState });
        }

        var user = (CurrentUser)HttpContext.Items["CurrentUser"]!;

        var evt = _db.Events.FirstOrDefault(e => e.Id == dto.EventId);
        if (evt == null)
        {
            return NotFound(new { message = "Event not found." });
        }

        if (evt.OrganiserId != user.Id)
        {
            return Forbid();
        }

        var category = new Category
        {
            EventId = evt.Id,
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim()
        };

        _db.Categories.Add(category);
        _db.SaveChanges();

        return CreatedAtAction(nameof(GetCategoriesForEvent), new { eventId = evt.Id }, new CategoryDto
        {
            Id = category.Id,
            EventId = category.EventId,
            EventName = evt.Name,
            Name = category.Name,
            Description = category.Description
        });
    }

    /// <summary>
    /// Delete a category (Organiser only - must own the event).
    /// </summary>
    [HttpDelete("{id}")]
    [OrganiserOnly]
    public IActionResult DeleteCategory(int id)
    {
        var user = (CurrentUser)HttpContext.Items["CurrentUser"]!;
        var category = _db.Categories.Include(c => c.Event).FirstOrDefault(c => c.Id == id);

        if (category == null)
        {
            return NotFound(new { message = "Category not found." });
        }

        if (category.Event.OrganiserId != user.Id)
        {
            return Forbid();
        }

        _db.Categories.Remove(category);
        _db.SaveChanges();
        return NoContent();
    }
}