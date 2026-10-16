namespace RaceDay.API.Models;

public enum EventType
{
    Run,
    Walk,
    Cycle
}

public class Event
{
    public int Id { get; set; }

    public int OrganiserId { get; set; }
    public User Organiser { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Location { get; set; } = string.Empty;
    public double Distance { get; set; }
    public EventType EventType { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Category> Categories { get; set; } = new List<Category>();
    public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
}