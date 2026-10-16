namespace RaceDay.API.Models;

public class Category
{
    public int Id { get; set; }

    public int EventId { get; set; }
    public Event Event { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
}