using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using RaceDay.API.Controllers;
using RaceDay.API.DTOs;
using RaceDay.API.Models;
using Xunit;

namespace RaceDay.Tests;

public class EnrolmentsControllerTests : TestBase
{
    private readonly EnrolmentsController _controller;

    public EnrolmentsControllerTests() : base()
    {
        _controller = new EnrolmentsController(Db);
    }

    private int SeedEventAndCategory(int organiserId)
    {
        var evt = new Event
        {
            Id = 1, OrganiserId = organiserId, Name = "10km Race", Description = "d",
            Date = DateTime.Today.AddDays(10), Location = "Park", Distance = 10.0, EventType = EventType.Run
        };
        Db.Events.Add(evt);
        var cat = new Category { Id = 1, EventId = 1, Name = "Senior", Description = "Over 18" };
        Db.Categories.Add(cat);
        Db.SaveChanges();
        return 1; // event id
    }

    [Fact]
    public void CreateEnrolment_ParticipantSuccess_ReturnsCreated()
    {
        SeedEventAndCategory(99);
        ControllerWithUser(_controller, 5, "part@example.com", "Participant Five", UserRole.Participant);

        var result = _controller.CreateEnrolment(new CreateEnrolmentDto { EventId = 1, CategoryId = 1 });
        var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.StatusCode.Should().Be(201);

        var enrolment = Db.Enrolments.Should().HaveCount(1).And.Subject.First();
        enrolment.ParticipantId.Should().Be(5);
        enrolment.CategoryId.Should().Be(1);
    }

    [Fact]
    public void CreateEnrolment_OrganiserRejected_Returns403()
    {
        SeedEventAndCategory(99);
        ControllerWithUser(_controller, 1, "org@example.com", "Organiser One", UserRole.Organiser);

        var result = _controller.CreateEnrolment(new CreateEnrolmentDto { EventId = 1, CategoryId = 1 });
        result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(403);
        Db.Enrolments.Should().BeEmpty();
    }

    [Fact]
    public void CreateEnrolment_Unauthenticated_Returns401()
    {
        SeedEventAndCategory(99);
        ControllerUnauthenticated(_controller);
        var result = _controller.CreateEnrolment(new CreateEnrolmentDto { EventId = 1, CategoryId = 1 });
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public void CreateEnrolment_Duplicate_ReturnsBadRequest()
    {
        SeedEventAndCategory(99);
        // enrol directly
        Db.Enrolments.Add(new Enrolment { EventId = 1, ParticipantId = 5, CategoryId = 1 });
        Db.SaveChanges();

        ControllerWithUser(_controller, 5, "part@example.com", "Participant Five", UserRole.Participant);
        var result = _controller.CreateEnrolment(new CreateEnrolmentDto { EventId = 1, CategoryId = 1 });
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public void GetMyEnrolments_Participant_ReturnsOwnEnrolments()
    {
        SeedEventAndCategory(99);
        Db.Enrolments.Add(new Enrolment { Id = 1, EventId = 1, ParticipantId = 5, CategoryId = 1 });
        Db.Enrolments.Add(new Enrolment { Id = 2, EventId = 1, ParticipantId = 6, CategoryId = 1 }); // other user's
        Db.SaveChanges();

        ControllerWithUser(_controller, 5, "part@example.com", "Participant Five", UserRole.Participant);
        var result = _controller.GetMyEnrolments();
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var list = ok.Value.Should().BeAssignableTo<IEnumerable<EnrolmentDto>>().Subject;
        list.Should().HaveCount(1);
        list.First().ParticipantId.Should().Be(5);
    }

    [Fact]
    public void GetMyEnrolments_Unauthenticated_Returns401()
    {
        ControllerUnauthenticated(_controller);
        _controller.GetMyEnrolments().Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public void GetEnrolmentsForEvent_OrganiserOwner_ReturnsAll()
    {
        SeedEventAndCategory(1); // organiser id 1 owns the event
        Db.Enrolments.Add(new Enrolment { Id = 1, EventId = 1, ParticipantId = 5, CategoryId = 1 });
        Db.Enrolments.Add(new Enrolment { Id = 2, EventId = 1, ParticipantId = 6, CategoryId = 1 });
        Db.SaveChanges();

        ControllerWithUser(_controller, 1, "org@example.com", "Organiser One", UserRole.Organiser);
        var result = _controller.GetEnrolmentsForEvent(1);
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var list = ok.Value.Should().BeAssignableTo<IEnumerable<EnrolmentDto>>().Subject;
        list.Should().HaveCount(2);
    }

    [Fact]
    public void GetEnrolmentsForEvent_OrganiserNotOwner_Returns403()
    {
        SeedEventAndCategory(1); // owned by user 1
        ControllerWithUser(_controller, 2, "org2@example.com", "Organiser Two", UserRole.Organiser);
        var result = _controller.GetEnrolmentsForEvent(1);
        result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(403);
    }

    [Fact]
    public void GetEnrolmentsForEvent_Unauthenticated_Returns401()
    {
        ControllerUnauthenticated(_controller);
        _controller.GetEnrolmentsForEvent(1).Should().BeOfType<UnauthorizedObjectResult>();
    }
}