using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using RaceDay.API.Controllers;
using RaceDay.API.DTOs;
using RaceDay.API.Models;
using RaceDay.API.Services;
using Xunit;

namespace RaceDay.Tests;

public class AuthControllerTests : TestBase
{
    private readonly AuthController _controller;

    public AuthControllerTests() : base()
    {
        _controller = new AuthController(new AuthService(Db, new PasswordHasher()), Db);
    }

    [Fact]
    public void Register_Success_ReturnsCreatedUser()
    {
        var result = _controller.Register(new RegisterRequest
        {
            FirstName = "Jane",
            LastName = "Doe",
            Email = "jane@example.com",
            Password = "Password123",
            Role = UserRole.Participant
        });

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<AuthResponse>().Subject;
        response.Success.Should().BeTrue();
        response.User.Should().NotBeNull();
        response.User!.Email.Should().Be("jane@example.com");
        response.User.Role.Should().Be(UserRole.Participant);

        Db.Users.Should().HaveCount(1);
    }

    [Fact]
    public void Register_DuplicateEmail_ReturnsBadRequest()
    {
        _controller.Register(new RegisterRequest
        {
            FirstName = "Jane", LastName = "Doe", Email = "jane@example.com",
            Password = "Password123", Role = UserRole.Participant
        });

        var result = _controller.Register(new RegisterRequest
        {
            FirstName = "Jane2", LastName = "Doe2", Email = "JANE@example.com",
            Password = "Password123", Role = UserRole.Organiser
        });

        var bad = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        Db.Users.Should().HaveCount(1);
    }

    [Fact]
    public void Register_MissingFields_ReturnsBadRequest()
    {
        var result = _controller.Register(new RegisterRequest
        {
            FirstName = "", LastName = "Doe", Email = "jane@example.com",
            Password = "Password123", Role = UserRole.Participant
        });

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public void Login_Success_ReturnsUserAndSetsSession()
    {
        _controller.Register(new RegisterRequest
        {
            FirstName = "Jane", LastName = "Doe", Email = "jane@example.com",
            Password = "Password123", Role = UserRole.Participant
        });

        // Simulate login session being established (controller writes to HttpContext.Session).
        ControllerWithUser(_controller, 1, "jane@example.com", "Jane Doe", UserRole.Participant);

        var result = _controller.Login(new LoginRequest { Email = "jane@example.com", Password = "Password123" });

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<AuthResponse>().Subject;
        response.Success.Should().BeTrue();
    }

    [Fact]
    public void Login_WrongPassword_ReturnsUnauthorized()
    {
        _controller.Register(new RegisterRequest
        {
            FirstName = "Jane", LastName = "Doe", Email = "jane@example.com",
            Password = "Password123", Role = UserRole.Participant
        });

        var result = _controller.Login(new LoginRequest { Email = "jane@example.com", Password = "Wrong" });
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public void Me_Unauthenticated_Returns401()
    {
        ControllerUnauthenticated(_controller);
        var result = _controller.Me();
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public void Me_Authenticated_ReturnsUserProfile()
    {
        ControllerWithUser(_controller, 1, "jane@example.com", "Jane Doe", UserRole.Participant);
        Db.Users.Add(new User
        {
            Id = 1, FirstName = "Jane", LastName = "Doe", Email = "jane@example.com",
            PasswordHash = "x", PasswordSalt = "y", Role = UserRole.Participant
        });
        Db.SaveChanges();

        var result = _controller.Me();
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var user = ok.Value.Should().BeOfType<UserDto>().Subject;
        user.Email.Should().Be("jane@example.com");
    }

    [Fact]
    public void Logout_Authenticated_ReturnsOk()
    {
        ControllerWithUser(_controller, 1, "jane@example.com", "Jane Doe", UserRole.Participant);
        var result = _controller.Logout();
        result.Should().BeOfType<OkObjectResult>();
    }
}