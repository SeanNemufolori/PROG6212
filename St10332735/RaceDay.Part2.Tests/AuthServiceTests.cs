using FluentAssertions;
using RaceDay.API.Data;
using RaceDay.API.Models;
using RaceDay.API.Services;
using Xunit;

namespace RaceDay.Tests;

public class AuthServiceTests : TestBase
{
    private readonly IPasswordHasher _hasher = new PasswordHasher();
    private readonly AuthService _service;

    public AuthServiceTests() : base()
    {
        _service = new AuthService(Db, _hasher);
    }

    [Fact]
    public void Register_Success_CreatesUserWithHashedPassword()
    {
        var (success, user, error) = _service.Register("Jane", "Doe", "jane@example.com", "Password123", UserRole.Participant);

        success.Should().BeTrue();
        error.Should().BeNull();
        user.Should().NotBeNull();
        user!.Email.Should().Be("jane@example.com");
        user.Role.Should().Be(UserRole.Participant);
        user.PasswordHash.Should().NotBe("Password123"); // hashed, not plaintext
        user.PasswordSalt.Should().NotBeNullOrEmpty();

        Db.Users.Should().HaveCount(1);
    }

    [Fact]
    public void Register_DuplicateEmail_Fails()
    {
        _service.Register("Jane", "Doe", "jane@example.com", "Password123", UserRole.Participant);
        var (success, _, error) = _service.Register("Jane", "Doe", "JANE@example.com", "Password123", UserRole.Organiser);

        success.Should().BeFalse();
        error.Should().Be("An account with this email already exists.");
        Db.Users.Should().HaveCount(1);
    }

    [Fact]
    public void Register_ShortPassword_Fails()
    {
        var (success, _, error) = _service.Register("Jane", "Doe", "jane@example.com", "123", UserRole.Participant);
        success.Should().BeFalse();
        error.Should().Be("Password must be at least 6 characters.");
    }

    [Fact]
    public void Register_MissingFields_Fails()
    {
        var (success, _, error) = _service.Register("", "Doe", "jane@example.com", "Password123", UserRole.Participant);
        success.Should().BeFalse();
        error.Should().Be("All fields are required.");
    }

    [Fact]
    public void Login_Success_ReturnsUser()
    {
        _service.Register("Jane", "Doe", "jane@example.com", "Password123", UserRole.Participant);
        var (success, user, error) = _service.Login("jane@example.com", "Password123");

        success.Should().BeTrue();
        error.Should().BeNull();
        user!.Email.Should().Be("jane@example.com");
    }

    [Fact]
    public void Login_WrongPassword_Fails()
    {
        _service.Register("Jane", "Doe", "jane@example.com", "Password123", UserRole.Participant);
        var (success, _, error) = _service.Login("jane@example.com", "WrongPassword");

        success.Should().BeFalse();
        error.Should().Be("Invalid email or password.");
    }

    [Fact]
    public void Login_UnknownEmail_Fails()
    {
        var (success, _, error) = _service.Login("none@example.com", "Password123");
        success.Should().BeFalse();
        error.Should().Be("Invalid email or password.");
    }

    [Fact]
    public void Login_CaseInsensitiveEmail_Success()
    {
        _service.Register("Jane", "Doe", "Jane@Example.com", "Password123", UserRole.Participant);
        var (success, _, _) = _service.Login("jane@example.com", "Password123");
        success.Should().BeTrue();
    }

    [Fact]
    public void Logout_ClearsSession()
    {
        var session = new TestSession();
        _service.Logout(session);
        session.Keys.Should().BeEmpty();
    }
}