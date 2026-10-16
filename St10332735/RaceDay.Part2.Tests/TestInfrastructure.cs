using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using RaceDay.API.Data;
using RaceDay.API.Models;
using RaceDay.API.Services;

namespace RaceDay.Tests;

public class TestBase : IDisposable
{
    protected readonly AppDbContext Db;

    public TestBase()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        Db = new AppDbContext(options);
    }

    public void Dispose() => Db.Dispose();

    /// <summary>
    /// Builds a controller with a faked authenticated CurrentUser in HttpContext.Items
    /// and a fake session, simulating what CurrentUserMiddleware does after session login.
    /// </summary>
    protected static TController ControllerWithUser<TController>(TController controller, int userId, string email, string name, UserRole role)
        where TController : ControllerBase
    {
        var httpContext = new DefaultHttpContext
        {
            Items =
            {
                ["CurrentUser"] = new CurrentUser { Id = userId, Email = email, FullName = name, Role = role }
            }
        };
        httpContext.Features.Set(new SessionFeature { Session = new TestSession() });
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        return controller;
    }

    /// <summary>
    /// Builds a controller with NO authenticated user (simulates unauthenticated request)
    /// and a fake session.
    /// </summary>
    protected static TController ControllerUnauthenticated<TController>(TController controller)
        where TController : ControllerBase
    {
        var httpContext = new DefaultHttpContext { Items = new Dictionary<object?, object?>() };
        httpContext.Features.Set(new SessionFeature { Session = new TestSession() });
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        return controller;
    }
}

/// <summary>
/// In-memory session stub so AuthService.Logout / controllers can read/write session without ASP.NET session middleware.
/// </summary>
public class TestSession : ISession
{
    private readonly Dictionary<string, byte[]> _store = new();
    public IEnumerable<string> Keys => _store.Keys;
    public bool IsAvailable => true;
    public void Clear() => _store.Clear();
    public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    public void Remove(string key) => _store.Remove(key);
    public void Set(string key, byte[] value) => _store[key] = value;
    public bool TryGetValue(string key, out byte[] value) => _store.TryGetValue(key, out value);
}