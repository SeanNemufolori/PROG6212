using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models;

namespace RaceDay.API.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly IPasswordHasher _passwordHasher;

    public AuthService(AppDbContext db, IPasswordHasher passwordHasher)
    {
        _db = db;
        _passwordHasher = passwordHasher;
    }

    public (bool Success, User? User, string? Error) Register(string firstName, string lastName, string email, string password, UserRole role)
    {
        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName) ||
            string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return (false, null, "All fields are required.");
        }

        if (_db.Users.Any(u => u.Email == email.Trim().ToLowerInvariant()))
        {
            return (false, null, "An account with this email already exists.");
        }

        if (password.Length < 6)
        {
            return (false, null, "Password must be at least 6 characters.");
        }

        var hash = _passwordHasher.Hash(password, out var salt);

        var user = new User
        {
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            Email = email.Trim().ToLowerInvariant(),
            PasswordHash = hash,
            PasswordSalt = salt,
            Role = role,
            CreatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);
        _db.SaveChanges();

        return (true, user, null);
    }

    public (bool Success, User? User, string? Error) Login(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return (false, null, "Email and password are required.");
        }

        var user = _db.Users.FirstOrDefault(u => u.Email == email.Trim().ToLowerInvariant());
        if (user == null)
        {
            return (false, null, "Invalid email or password.");
        }

        if (!_passwordHasher.Verify(password, user.PasswordSalt, user.PasswordHash))
        {
            return (false, null, "Invalid email or password.");
        }

        return (true, user, null);
    }

    public void Logout(Microsoft.AspNetCore.Http.ISession session)
    {
        session.Clear();
    }

    public CurrentUser? GetCurrentUser(Microsoft.AspNetCore.Http.ISession session)
    {
        var userId = session.GetInt32("UserId");
        if (!userId.HasValue)
        {
            return null;
        }

        return new CurrentUser
        {
            Id = userId.Value,
            Email = session.GetString("UserEmail") ?? string.Empty,
            FullName = session.GetString("UserName") ?? string.Empty,
            Role = Enum.TryParse<UserRole>(session.GetString("UserRole"), out var role) ? role : UserRole.Participant
        };
    }
}