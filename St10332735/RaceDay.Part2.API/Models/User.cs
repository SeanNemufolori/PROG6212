namespace RaceDay.API.Models;

public enum UserRole
{
    Participant,
    Organiser
}

public class User
{
    public int Id { get; set; }

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string PasswordSalt { get; set; } = string.Empty;

    public UserRole Role { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string FullName => $"{FirstName} {LastName}".Trim();

    public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
    public ICollection<Event> OrganisedEvents { get; set; } = new List<Event>();
    public ICollection<Result> CapturedResults { get; set; } = new List<Result>();
}