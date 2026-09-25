namespace CodeRoom.Web.Models;

public class Lesson
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? VideoUrl { get; set; }
    public string? ResourceUrl { get; set; }
    public int Order { get; set; }
    public bool IsPublished { get; set; } = true;

    public Course Course { get; set; } = null!;
}
