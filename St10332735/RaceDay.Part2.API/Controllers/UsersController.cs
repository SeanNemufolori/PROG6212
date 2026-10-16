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
public class UsersController : ControllerBase
{
    private readonly AppDbContext _db;

    public UsersController(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// View your own profile (authenticated users).
    /// </summary>
    [HttpGet("me")]
    [Authenticated]
    public IActionResult GetProfile()
    {
        var user = (CurrentUser)HttpContext.Items["CurrentUser"]!;
        var dbUser = _db.Users.First(u => u.Id == user.Id);
        return Ok(new UserDto
        {
            Id = dbUser.Id,
            FirstName = dbUser.FirstName,
            LastName = dbUser.LastName,
            Email = dbUser.Email,
            FullName = dbUser.FullName,
            Role = dbUser.Role,
            CreatedAt = dbUser.CreatedAt
        });
    }

    /// <summary>
    /// Update your own profile (authenticated users).
    /// </summary>
    [HttpPut("me")]
    [Authenticated]
    public IActionResult UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        var user = (CurrentUser)HttpContext.Items["CurrentUser"]!;
        var dbUser = _db.Users.First(u => u.Id == user.Id);

        if (!string.IsNullOrWhiteSpace(dto.FirstName))
        {
            dbUser.FirstName = dto.FirstName.Trim();
        }

        if (!string.IsNullOrWhiteSpace(dto.LastName))
        {
            dbUser.LastName = dto.LastName.Trim();
        }

        if (!string.IsNullOrWhiteSpace(dto.Email))
        {
            var email = dto.Email.Trim().ToLowerInvariant();
            if (_db.Users.Any(u => u.Id != dbUser.Id && u.Email == email))
            {
                return BadRequest(new { message = "Email already in use." });
            }
            dbUser.Email = email;
            HttpContext.Session.SetString("UserEmail", dbUser.Email);
        }

        _db.SaveChanges();

        return Ok(new UserDto
        {
            Id = dbUser.Id,
            FirstName = dbUser.FirstName,
            LastName = dbUser.LastName,
            Email = dbUser.Email,
            FullName = dbUser.FullName,
            Role = dbUser.Role,
            CreatedAt = dbUser.CreatedAt
        });
    }
}