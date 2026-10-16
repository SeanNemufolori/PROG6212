using RaceDay.API.Models;

namespace RaceDay.API.Services;

public interface IAuthService
{
    (bool Success, User? User, string? Error) Register(string firstName, string lastName, string email, string password, UserRole role);
    (bool Success, User? User, string? Error) Login(string email, string password);
    void Logout(Microsoft.AspNetCore.Http.ISession session);
    CurrentUser? GetCurrentUser(Microsoft.AspNetCore.Http.ISession session);
}