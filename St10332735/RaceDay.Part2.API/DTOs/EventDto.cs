using RaceDay.API.Models;

namespace RaceDay.API.DTOs;

public class EventDto
{
    public int Id { get; set; }
    public int OrganiserId { get; set; }
    public string OrganiserName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Location { get; set; } = string.Empty;
    public double Distance { get; set; }
    public EventType EventType { get; set; }
    public DateTime CreatedAt { get; set; }
    public int CategoryCount { get; set; }
    public int EnrolmentCount { get; set; }
}

public class CreateEventDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Location { get; set; } = string.Empty;
    public double Distance { get; set; }
    public EventType EventType { get; set; }
}

public class UpdateEventDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public DateTime? Date { get; set; }
    public string? Location { get; set; }
    public double? Distance { get; set; }
    public EventType? EventType { get; set; }
}