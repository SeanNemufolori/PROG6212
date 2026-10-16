using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using RaceDay.API.Controllers;
using RaceDay.API.DTOs;
using RaceDay.API.Models;
using Xunit;

namespace RaceDay.Tests;

public class ResultsControllerTests : TestBase
{
    private readonly ResultsController _controller;

    public ResultsControllerTests() : base()
    {
        _controller = new ResultsController(Db);
    }

    private void SeedEnrolment(int organiserId, int participantId)
    {
        var evt = new Event
        {
            Id = 1, OrganiserId = organiserId, Name = "10km Race", Description = "d",
            Date = DateTime.Today.AddDays(10), Location = "Park", Distance = 10.0, EventType = EventType.Run
        };
        var cat = new Category { Id = 1, EventId = 1, Name = "Senior", Description = "Over 18" };
        var enrolment = new Enrolment
        {
            Id = 1, EventId = 1, ParticipantId = participantId, CategoryId = 1,
            Participant = new User { Id = participantId, Email = "part@example.com" },
            Category = cat,
            Event = evt
        };
        Db.Events.Add(evt);
        Db.Categories.Add(cat);
        Db.Users.Add(enrolment.Participant);
        Db.Enrolments.Add(enrolment);
        Db.SaveChanges();
    }

    [Fact]
    public void CaptureResult_OrganiserSuccess_ReturnsCreated()
    {
        SeedEnrolment(organiserId: 1, participantId: 5);
        ControllerWithUser(_controller, 1, "org@example.com", "Organiser One", UserRole.Organiser);

        var result = _controller.CaptureResult(new CreateResultDto
        {
            EnrolmentId = 1,
            FinishTime = TimeSpan.FromMinutes(45),
            FinishingPosition = 3
        });

        var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.StatusCode.Should().Be(201);

        Db.Results.Should().HaveCount(1);
        Db.Results.First().CapturedBy.Should().Be(1);
        Db.Results.First().FinishingPosition.Should().Be(3);
    }

    [Fact]
    public void CaptureResult_ParticipantRejected_Returns403()
    {
        SeedEnrolment(organiserId: 1, participantId: 5);
        ControllerWithUser(_controller, 5, "part@example.com", "Participant Five", UserRole.Participant);

        var result = _controller.CaptureResult(new CreateResultDto
        {
            EnrolmentId = 1, FinishTime = TimeSpan.FromMinutes(45), FinishingPosition = 3
        });
        result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(403);
        Db.Results.Should().BeEmpty();
    }

    [Fact]
    public void CaptureResult_Unauthenticated_Returns401()
    {
        SeedEnrolment(organiserId: 1, participantId: 5);
        ControllerUnauthenticated(_controller);
        var result = _controller.CaptureResult(new CreateResultDto
        {
            EnrolmentId = 1, FinishTime = TimeSpan.FromMinutes(45), FinishingPosition = 3
        });
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public void CaptureResult_NotOwnerOrganiser_Returns403()
    {
        SeedEnrolment(organiserId: 1, participantId: 5); // owned by user 1
        ControllerWithUser(_controller, 2, "org2@example.com", "Organiser Two", UserRole.Organiser);
        var result = _controller.CaptureResult(new CreateResultDto
        {
            EnrolmentId = 1, FinishTime = TimeSpan.FromMinutes(45), FinishingPosition = 3
        });
        result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(403);
    }

    [Fact]
    public void CaptureResult_Duplicate_ReturnsBadRequest()
    {
        SeedEnrolment(organiserId: 1, participantId: 5);
        Db.Results.Add(new Result { EnrolmentId = 1, FinishTime = TimeSpan.FromMinutes(40), FinishingPosition = 1 });
        Db.SaveChanges();

        ControllerWithUser(_controller, 1, "org@example.com", "Organiser One", UserRole.Organiser);
        var result = _controller.CaptureResult(new CreateResultDto
        {
            EnrolmentId = 1, FinishTime = TimeSpan.FromMinutes(45), FinishingPosition = 3
        });
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public void GetMyResults_Participant_ReturnsOwnResults()
    {
        SeedEnrolment(organiserId: 1, participantId: 5);
        Db.Results.Add(new Result
        {
            Id = 1, EnrolmentId = 1, FinishTime = TimeSpan.FromMinutes(45), FinishingPosition = 3
        });
        Db.SaveChanges();

        ControllerWithUser(_controller, 5, "part@example.com", "Participant Five", UserRole.Participant);
        var result = _controller.GetMyResults();
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var list = ok.Value.Should().BeAssignableTo<IEnumerable<ResultDto>>().Subject;
        list.Should().HaveCount(1);
        list.First().FinishingPosition.Should().Be(3);
    }

    [Fact]
    public void GetMyResults_Unauthenticated_Returns401()
    {
        ControllerUnauthenticated(_controller);
        _controller.GetMyResults().Should().BeOfType<UnauthorizedObjectResult>();
    }
}