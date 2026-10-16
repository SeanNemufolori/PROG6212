using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using RaceDay.API.Controllers;
using RaceDay.API.DTOs;
using RaceDay.API.Models;
using Xunit;

namespace RaceDay.Tests;

public class UsersControllerTests : TestBase
{
    private readonly UsersController _controller;

    public UsersControllerTests() : base()
    {
        _controller = new UsersController(Db);
    }

    [Fact]
    public void GetProfile_Authenticated_ReturnsUser()
    {
        var user = new User { Id = 1, FirstName = "Jane", LastName = "Doe", Email = "jane@example.com", Role = UserRole.Participant };
        Db.Users.Add(user);
        Db.SaveChanges();

        ControllerWithUser(_controller, 1, "jane@example.com", "Jane Doe", UserRole.Participant);

        var result = _controller.GetProfile();
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var dto = ok.Value.Should().BeOfType<UserDto>().Subject;
        dto.Email.Should().Be("jane@example.com");
        dto.Role.Should().Be(UserRole.Participant);
    }

    [Fact]
    public void GetProfile_Unauthenticated_Returns401()
    {
        ControllerUnauthenticated(_controller);
        _controller.GetProfile().Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public void UpdateProfile_Authenticated_Success()
    {
        var user = new User { Id = 1, FirstName = "Jane", LastName = "Doe", Email = "jane@example.com", Role = UserRole.Participant };
        Db.Users.Add(user);
        Db.SaveChanges();

        ControllerWithUser(_controller, 1, "jane@example.com", "Jane Doe", UserRole.Participant);

        var result = _controller.UpdateProfile(new UpdateProfileDto { FirstName = "Janet" });
        result.Should().BeOfType<OkObjectResult>();
        Db.Users.First().FirstName.Should().Be("Janet");
    }

    [Fact]
    public void UpdateProfile_DuplicateEmail_ReturnsBadRequest()
    {
        Db.Users.Add(new User { Id = 1, FirstName = "Jane", LastName = "Doe", Email = "jane@example.com", Role = UserRole.Participant });
        Db.Users.Add(new User { Id = 2, FirstName = "Bob", LastName = "Smith", Email = "bob@example.com", Role = UserRole.Participant });
        Db.SaveChanges();

        ControllerWithUser(_controller, 1, "jane@example.com", "Jane Doe", UserRole.Participant);

        var result = _controller.UpdateProfile(new UpdateProfileDto { Email = "bob@example.com" });
        var bad = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        bad.Value.Should().BeOfType<object>();
    }

    [Fact]
    public void UpdateProfile_Unauthenticated_Returns401()
    {
        ControllerUnauthenticated(_controller);
        _controller.UpdateProfile(new UpdateProfileDto()).Should().BeOfType<UnauthorizedObjectResult>();
    }
}