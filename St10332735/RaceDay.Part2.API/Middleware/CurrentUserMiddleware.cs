using RaceDay.API.Models;
using RaceDay.API.Services;

namespace RaceDay.API.Middleware;

public class CurrentUserMiddleware
{
    private readonly RequestDelegate _next;

    public CurrentUserMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IAuthService authService)
    {
        var currentUser = authService.GetCurrentUser(context.Session);
        if (currentUser != null)
        {
            context.Items["CurrentUser"] = currentUser;
        }

        await _next(context);
    }
}