namespace RaceDay.API.Models;

public class Enrolment
{
    public int Id { get; set; }

    public int EventId { get; set; }
    public Event Event { get; set; } = null!;

    public int ParticipantId { get; set; }
    public User Participant { get; set; } = null!;

    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;

    public Result? Result { get; set; }
}