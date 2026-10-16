namespace RaceDay.API.DTOs;

public class ResultDto
{
    public int Id { get; set; }
    public int EnrolmentId { get; set; }
    public int EventId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public int ParticipantId { get; set; }
    public string ParticipantEmail { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public TimeSpan FinishTime { get; set; }
    public int FinishingPosition { get; set; }
    public DateTime CapturedAt { get; set; }
    public int? CapturedBy { get; set; }
}

public class CreateResultDto
{
    public int EnrolmentId { get; set; }
    public TimeSpan FinishTime { get; set; }
    public int FinishingPosition { get; set; }
}