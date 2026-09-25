namespace CodeRoom.Web.Models;

public class Quiz
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public Course Course { get; set; } = null!;
    public ICollection<Question> Questions { get; set; } = [];
}
