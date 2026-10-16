namespace RaceDay.API.DTOs;

public class EnrolmentDto
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public int ParticipantId { get; set; }
    public string ParticipantEmail { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public DateTime EnrolledAt { get; set; }
    public bool HasResult { get; set; }
}

public class CreateEnrolmentDto
{
    public int EventId { get; set; }
    public int CategoryId { get; set; }
}