namespace RaceDay.API.DTOs;

public class CategoryDto
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class CreateCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}