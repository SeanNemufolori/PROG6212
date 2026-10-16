using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Filters;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ResultsController : ControllerBase
{
    private readonly AppDbContext _db;

    public ResultsController(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Capture a finish time and position for a participant's enrolment (Organiser only - must own the event).
    /// </summary>
    [HttpPost]
    [OrganiserOnly]
    public IActionResult CaptureResult([FromBody] CreateResultDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { message = "Invalid result data.", errors = ModelState });
        }

        var user = (CurrentUser)HttpContext.Items["CurrentUser"]!;

        var enrolment = _db.Enrolments
            .Include(e => e.Event)
            .Include(e => e.Participant)
            .Include(e => e.Category)
            .FirstOrDefault(e => e.Id == dto.EnrolmentId);

        if (enrolment == null)
        {
            return NotFound(new { message = "Enrolment not found." });
        }

        if (enrolment.Event.OrganiserId != user.Id)
        {
            return Forbid();
        }

        if (enrolment.Result != null)
        {
            return BadRequest(new { message = "A result already exists for this enrolment." });
        }

        var result = new Result
        {
            EnrolmentId = dto.EnrolmentId,
            FinishTime = dto.FinishTime,
            FinishingPosition = dto.FinishingPosition,
            CapturedAt = DateTime.UtcNow,
            CapturedBy = user.Id
        };

        _db.Results.Add(result);
        _db.SaveChanges();

        return CreatedAtAction(nameof(GetMyResults), new { }, new ResultDto
        {
            Id = result.Id,
            EnrolmentId = result.EnrolmentId,
            EventId = enrolment.Event.Id,
            EventName = enrolment.Event.Name,
            ParticipantId = enrolment.ParticipantId,
            ParticipantEmail = enrolment.Participant.Email,
            CategoryName = enrolment.Category.Name,
            FinishTime = result.FinishTime,
            FinishingPosition = result.FinishingPosition,
            CapturedAt = result.CapturedAt,
            CapturedBy = result.CapturedBy
        });
    }

    /// <summary>
    /// View the current participant's own results (Participant only).
    /// </summary>
    [HttpGet("mine")]
    [ParticipantOnly]
    public IActionResult GetMyResults()
    {
        var user = (CurrentUser)HttpContext.Items["CurrentUser"]!;

        var results = _db.Results
            .Include(r => r.Enrolment)
                .ThenInclude(e => e.Event)
            .Include(r => r.Enrolment)
                .ThenInclude(e => e.Category)
            .Where(r => r.Enrolment.ParticipantId == user.Id)
            .Select(r => new ResultDto
            {
                Id = r.Id,
                EnrolmentId = r.EnrolmentId,
                EventId = r.Enrolment.Event.Id,
                EventName = r.Enrolment.Event.Name,
                ParticipantId = r.Enrolment.ParticipantId,
                ParticipantEmail = user.Email,
                CategoryName = r.Enrolment.Category.Name,
                FinishTime = r.FinishTime,
                FinishingPosition = r.FinishingPosition,
                CapturedAt = r.CapturedAt,
                CapturedBy = r.CapturedBy
            })
            .ToList();

        return Ok(results);
    }
}