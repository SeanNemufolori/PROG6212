using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using RaceDay.API.Data;
using RaceDay.API.Filters;
using RaceDay.API.Middleware;
using RaceDay.API.Services;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers(options =>
{
    // Role-based session authorization is enforced by custom filter attributes.
}).AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// Entity Framework Core - SQL Server (Code-First)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Swagger - all endpoints visible and testable through the UI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "RaceDay API",
        Version = "v1",
        Description = "RESTful API for the RaceDay event management system. Supports Organiser and Participant roles. Authentication is session-based (cookie)."
    });
});

// Session management for authenticated state and role
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
});

// Application services
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IAuthService, AuthService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "RaceDay API v1");
        c.RoutePrefix = string.Empty; // Serve Swagger UI at the root
    });
}

app.UseHttpsRedirection();
app.UseRouting();

// Session must be available before the custom middleware reads it
app.UseSession();

// Custom middleware: attach the authenticated user (from session) to HttpContext.Items
app.UseMiddleware<CurrentUserMiddleware>();

app.MapControllers();

// Ensure database is created and seeded on startup (Code-First)
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.EnsureCreated();
}

app.Run();