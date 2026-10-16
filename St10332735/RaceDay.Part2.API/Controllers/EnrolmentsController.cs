using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Filters;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EnrolmentsController : ControllerBase
{
    private readonly AppDbContext _db;

    public EnrolmentsController(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Enrol the current participant in an event with a selected category (Participant only).
    /// </summary>
    [HttpPost]
    [ParticipantOnly]
    public IActionResult CreateEnrolment([FromBody] CreateEnrolmentDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { message = "Invalid enrolment data.", errors = ModelState });
        }

        var user = (CurrentUser)HttpContext.Items["CurrentUser"]!;

        var evt = _db.Events.Include(e => e.Categories).FirstOrDefault(e => e.Id == dto.EventId);
        if (evt == null)
        {
            return NotFound(new { message = "Event not found." });
        }

        var category = evt.Categories.FirstOrDefault(c => c.Id == dto.CategoryId);
        if (category == null)
        {
            return BadRequest(new { message = "Category not found for this event." });
        }

        var alreadyEnrolled = _db.Enrolments.Any(e => e.EventId == dto.EventId && e.ParticipantId == user.Id);
        if (alreadyEnrolled)
        {
            return BadRequest(new { message = "You are already enrolled in this event." });
        }

        var enrolment = new Enrolment
        {
            EventId = dto.EventId,
            ParticipantId = user.Id,
            CategoryId = dto.CategoryId,
            EnrolledAt = DateTime.UtcNow
        };

        _db.Enrolments.Add(enrolment);
        _db.SaveChanges();

        return CreatedAtAction(nameof(GetMyEnrolments), new { }, new EnrolmentDto
        {
            Id = enrolment.Id,
            EventId = enrolment.EventId,
            EventName = evt.Name,
            ParticipantId = enrolment.ParticipantId,
            ParticipantEmail = user.Email,
            CategoryId = enrolment.CategoryId,
            CategoryName = category.Name,
            EnrolledAt = enrolment.EnrolledAt,
            HasResult = false
        });
    }

    /// <summary>
    /// View the current participant's enrolments (Participant only).
    /// </summary>
    [HttpGet("mine")]
    [ParticipantOnly]
    public IActionResult GetMyEnrolments()
    {
        var user = (CurrentUser)HttpContext.Items["CurrentUser"]!;

        var enrolments = _db.Enrolments
            .Include(e => e.Event)
            .Include(e => e.Category)
            .Include(e => e.Result)
            .Where(e => e.ParticipantId == user.Id)
            .Select(e => new EnrolmentDto
            {
                Id = e.Id,
                EventId = e.EventId,
                EventName = e.Event.Name,
                ParticipantId = e.ParticipantId,
                ParticipantEmail = user.Email,
                CategoryId = e.CategoryId,
                CategoryName = e.Category.Name,
                EnrolledAt = e.EnrolledAt,
                HasResult = e.Result != null
            })
            .ToList();

        return Ok(enrolments);
    }

    /// <summary>
    /// View all enrolments for an event (Organiser only - must own the event).
    /// </summary>
    [HttpGet("event/{eventId}")]
    [OrganiserOnly]
    public IActionResult GetEnrolmentsForEvent(int eventId)
    {
        var user = (CurrentUser)HttpContext.Items["CurrentUser"]!;

        var evt = _db.Events.FirstOrDefault(e => e.Id == eventId);
        if (evt == null)
        {
            return NotFound(new { message = "Event not found." });
        }

        if (evt.OrganiserId != user.Id)
        {
            return Forbid();
        }

        var enrolments = _db.Enrolments
            .Include(e => e.Participant)
            .Include(e => e.Category)
            .Where(e => e.EventId == eventId)
            .Select(e => new EnrolmentDto
            {
                Id = e.Id,
                EventId = e.EventId,
                EventName = evt.Name,
                ParticipantId = e.ParticipantId,
                ParticipantEmail = e.Participant.Email,
                CategoryId = e.CategoryId,
                CategoryName = e.Category.Name,
                EnrolledAt = e.EnrolledAt,
                HasResult = e.Result != null
            })
            .ToList();

        return Ok(enrolments);
    }
}