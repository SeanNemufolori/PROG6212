using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Filters;
using RaceDay.API.Models;
using RaceDay.API.Services;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly AppDbContext _db;

    public EventsController(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// View all events (public - both roles).
    /// </summary>
    [HttpGet]
    public IActionResult GetEvents()
    {
        var events = _db.Events
            .Include(e => e.Categories)
            .Include(e => e.Enrolments)
            .OrderByDescending(e => e.CreatedAt)
            .Select(e => new EventDto
            {
                Id = e.Id,
                OrganiserId = e.OrganiserId,
                OrganiserName = e.Organiser.FullName,
                Name = e.Name,
                Description = e.Description,
                Date = e.Date,
                Location = e.Location,
                Distance = e.Distance,
                EventType = e.EventType,
                CreatedAt = e.CreatedAt,
                CategoryCount = e.Categories.Count,
                EnrolmentCount = e.Enrolments.Count
            })
            .ToList();

        return Ok(events);
    }

    /// <summary>
    /// View a single event by id (public).
    /// </summary>
    [HttpGet("{id}")]
    public IActionResult GetEvent(int id)
    {
        var e = _db.Events
            .Include(x => x.Categories)
            .Include(x => x.Enrolments)
            .FirstOrDefault(x => x.Id == id);

        if (e == null)
        {
            return NotFound(new { message = "Event not found." });
        }

        return Ok(new EventDto
        {
            Id = e.Id,
            OrganiserId = e.OrganiserId,
            OrganiserName = e.Organiser.FullName,
            Name = e.Name,
            Description = e.Description,
            Date = e.Date,
            Location = e.Location,
            Distance = e.Distance,
            EventType = e.EventType,
            CreatedAt = e.CreatedAt,
            CategoryCount = e.Categories.Count,
            EnrolmentCount = e.Enrolments.Count
        });
    }

    /// <summary>
    /// Create an event (Organiser only).
    /// </summary>
    [HttpPost]
    [OrganiserOnly]
    public IActionResult CreateEvent([FromBody] CreateEventDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { message = "Invalid event data.", errors = ModelState });
        }

        var user = (CurrentUser)HttpContext.Items["CurrentUser"]!;

        var evt = new Event
        {
            OrganiserId = user.Id,
            Name = dto.Name.Trim(),
            Description = dto.Description.Trim(),
            Date = dto.Date,
            Location = dto.Location.Trim(),
            Distance = dto.Distance,
            EventType = dto.EventType,
            CreatedAt = DateTime.UtcNow
        };

        _db.Events.Add(evt);
        _db.SaveChanges();

        return CreatedAtAction(nameof(GetEvent), new { id = evt.Id }, new EventDto
        {
            Id = evt.Id,
            OrganiserId = evt.OrganiserId,
            OrganiserName = user.FullName,
            Name = evt.Name,
            Description = evt.Description,
            Date = evt.Date,
            Location = evt.Location,
            Distance = evt.Distance,
            EventType = evt.EventType,
            CreatedAt = evt.CreatedAt,
            CategoryCount = 0,
            EnrolmentCount = 0
        });
    }

    /// <summary>
    /// Update an event (Organiser only - must own the event).
    /// </summary>
    [HttpPut("{id}")]
    [OrganiserOnly]
    public IActionResult UpdateEvent(int id, [FromBody] UpdateEventDto dto)
    {
        var user = (CurrentUser)HttpContext.Items["CurrentUser"]!;
        var evt = _db.Events.FirstOrDefault(e => e.Id == id);

        if (evt == null)
        {
            return NotFound(new { message = "Event not found." });
        }

        if (evt.OrganiserId != user.Id)
        {
            return Forbid();
        }

        if (dto.Name != null) evt.Name = dto.Name.Trim();
        if (dto.Description != null) evt.Description = dto.Description.Trim();
        if (dto.Date != null) evt.Date = dto.Date.Value;
        if (dto.Location != null) evt.Location = dto.Location.Trim();
        if (dto.Distance != null) evt.Distance = dto.Distance.Value;
        if (dto.EventType != null) evt.EventType = dto.EventType.Value;

        _db.SaveChanges();
        return NoContent();
    }

    /// <summary>
    /// Delete an event (Organiser only - must own the event).
    /// </summary>
    [HttpDelete("{id}")]
    [OrganiserOnly]
    public IActionResult DeleteEvent(int id)
    {
        var user = (CurrentUser)HttpContext.Items["CurrentUser"]!;
        var evt = _db.Events.FirstOrDefault(e => e.Id == id);

        if (evt == null)
        {
            return NotFound(new { message = "Event not found." });
        }

        if (evt.OrganiserId != user.Id)
        {
            return Forbid();
        }

        _db.Events.Remove(evt);
        _db.SaveChanges();
        return NoContent();
    }
}