using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using RaceDay.API.Controllers;
using RaceDay.API.DTOs;
using RaceDay.API.Models;
using Xunit;

namespace RaceDay.Tests;

public class EventsControllerTests : TestBase
{
    private readonly EventsController _controller;

    public EventsControllerTests() : base()
    {
        _controller = new EventsController(Db);
    }

    [Fact]
    public void GetEvents_Public_ReturnsAllEvents()
    {
        Db.Events.Add(new Event
        {
            Id = 1, OrganiserId = 1, Name = "Morning Run", Description = "5km run",
            Date = DateTime.Today.AddDays(7), Location = "Park", Distance = 5.0, EventType = EventType.Run
        });
        Db.SaveChanges();

        var result = _controller.GetEvents();
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var events = ok.Value.Should().BeAssignableTo<IEnumerable<EventDto>>().Subject;
        events.Should().HaveCount(1);
    }

    [Fact]
    public void GetEvent_ById_Public_ReturnsEvent()
    {
        Db.Events.Add(new Event
        {
            Id = 1, OrganiserId = 1, Name = "Morning Run", Description = "5km run",
            Date = DateTime.Today.AddDays(7), Location = "Park", Distance = 5.0, EventType = EventType.Run
        });
        Db.SaveChanges();

        var result = _controller.GetEvent(1);
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var evt = ok.Value.Should().BeOfType<EventDto>().Subject;
        evt.Name.Should().Be("Morning Run");
    }

    [Fact]
    public void CreateEvent_OrganiserSuccess_ReturnsCreated()
    {
        ControllerWithUser(_controller, 1, "org@example.com", "Organiser One", UserRole.Organiser);

        var result = _controller.CreateEvent(new CreateEventDto
        {
            Name = "Charity Run",
            Description = "A fun run",
            Date = DateTime.Today.AddDays(14),
            Location = "Stadium",
            Distance = 10.0,
            EventType = EventType.Run
        });

        var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.StatusCode.Should().Be(201);
        Db.Events.Should().HaveCount(1);
        Db.Events.First().OrganiserId.Should().Be(1);
    }

    [Fact]
    public void CreateEvent_ParticipantRejected_Returns403()
    {
        ControllerWithUser(_controller, 2, "part@example.com", "Participant Two", UserRole.Participant);

        var result = _controller.CreateEvent(new CreateEventDto
        {
            Name = "X", Description = "d", Date = DateTime.Today, Location = "L", Distance = 1, EventType = EventType.Walk
        });

        result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(403);
        Db.Events.Should().BeEmpty();
    }

    [Fact]
    public void CreateEvent_Unauthenticated_Returns401()
    {
        ControllerUnauthenticated(_controller);
        var result = _controller.CreateEvent(new CreateEventDto
        {
            Name = "X", Description = "d", Date = DateTime.Today, Location = "L", Distance = 1, EventType = EventType.Walk
        });
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public void UpdateEvent_OwnerOrganiserSuccess_ReturnsNoContent()
    {
        var evt = new Event
        {
            Id = 1, OrganiserId = 1, Name = "Old", Description = "d",
            Date = DateTime.Today, Location = "L", Distance = 5, EventType = EventType.Run
        };
        Db.Events.Add(evt);
        Db.SaveChanges();

        ControllerWithUser(_controller, 1, "org@example.com", "Organiser One", UserRole.Organiser);

        var result = _controller.UpdateEvent(1, new UpdateEventDto { Name = "New Name" });
        result.Should().BeOfType<NoContentResult>();

        Db.Events.First().Name.Should().Be("New Name");
    }

    [Fact]
    public void UpdateEvent_NotOwnerOrganiserRejected_Returns403()
    {
        var evt = new Event { Id = 1, OrganiserId = 99, Name = "Old", Description = "d", Date = DateTime.Today, Location = "L", Distance = 5, EventType = EventType.Run };
        Db.Events.Add(evt);
        Db.SaveChanges();

        ControllerWithUser(_controller, 1, "org@example.com", "Organiser One", UserRole.Organiser);

        var result = _controller.UpdateEvent(1, new UpdateEventDto { Name = "New Name" });
        result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(403);
    }

    [Fact]
    public void UpdateEvent_Unauthenticated_Returns401()
    {
        ControllerUnauthenticated(_controller);
        var result = _controller.UpdateEvent(1, new UpdateEventDto { Name = "New" });
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public void DeleteEvent_OwnerOrganiserSuccess_ReturnsNoContent()
    {
        var evt = new Event { Id = 1, OrganiserId = 1, Name = "Old", Description = "d", Date = DateTime.Today, Location = "L", Distance = 5, EventType = EventType.Run };
        Db.Events.Add(evt);
        Db.SaveChanges();

        ControllerWithUser(_controller, 1, "org@example.com", "Organiser One", UserRole.Organiser);
        var result = _controller.DeleteEvent(1);
        result.Should().BeOfType<NoContentResult>();
        Db.Events.Should().BeEmpty();
    }

    [Fact]
    public void DeleteEvent_Unauthenticated_Returns401()
    {
        ControllerUnauthenticated(_controller);
        var result = _controller.DeleteEvent(1);
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }
}