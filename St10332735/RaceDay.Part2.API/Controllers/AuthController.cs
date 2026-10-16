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
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly AppDbContext _db;

    public AuthController(IAuthService authService, AppDbContext db)
    {
        _authService = authService;
        _db = db;
    }

    /// <summary>
    /// Register a new account as Organiser or Participant.
    /// </summary>
    [HttpPost("register")]
    public IActionResult Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { message = "Invalid registration data.", errors = ModelState });
        }

        var (success, user, error) = _authService.Register(
            request.FirstName, request.LastName, request.Email, request.Password, request.Role);

        if (!success)
        {
            return BadRequest(new { message = error });
        }

        return Ok(new AuthResponse
        {
            Success = true,
            Message = "Registration successful.",
            User = ToUserDto(user!)
        });
    }

    /// <summary>
    /// Log in with email and password. Creates a session storing the user id, email, name and role.
    /// </summary>
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { message = "Invalid login data.", errors = ModelState });
        }

        var (success, user, error) = _authService.Login(request.Email, request.Password);

        if (!success)
        {
            return Unauthorized(new { message = error });
        }

        HttpContext.Session.SetInt32("UserId", user!.Id);
        HttpContext.Session.SetString("UserEmail", user.Email);
        HttpContext.Session.SetString("UserName", user.FullName);
        HttpContext.Session.SetString("UserRole", user.Role.ToString());

        return Ok(new AuthResponse
        {
            Success = true,
            Message = "Login successful.",
            User = ToUserDto(user)
        });
    }

    /// <summary>
    /// Log out and clear the session.
    /// </summary>
    [HttpPost("logout")]
    [Authenticated]
    public IActionResult Logout()
    {
        _authService.Logout(HttpContext.Session);
        return Ok(new { message = "Logged out successfully." });
    }

    /// <summary>
    /// Get the currently authenticated user's profile.
    /// </summary>
    [HttpGet("me")]
    [Authenticated]
    public IActionResult Me()
    {
        var user = (CurrentUser)HttpContext.Items["CurrentUser"]!;
        var dbUser = _db.Users.First(u => u.Id == user.Id);
        return Ok(ToUserDto(dbUser));
    }

    private static UserDto ToUserDto(User u) => new()
    {
        Id = u.Id,
        FirstName = u.FirstName,
        LastName = u.LastName,
        Email = u.Email,
        FullName = u.FullName,
        Role = u.Role,
        CreatedAt = u.CreatedAt
    };
}