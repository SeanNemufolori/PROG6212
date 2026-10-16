namespace RaceDay.API.Models;

public class Result
{
    public int Id { get; set; }

    public int EnrolmentId { get; set; }
    public Enrolment Enrolment { get; set; } = null!;

    public TimeSpan FinishTime { get; set; }
    public int FinishingPosition { get; set; }
    public DateTime CapturedAt { get; set; } = DateTime.UtcNow;

    public int? CapturedBy { get; set; }
    public User? CapturedByUser { get; set; }
}