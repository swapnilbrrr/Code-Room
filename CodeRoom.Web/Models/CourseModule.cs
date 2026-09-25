using System.ComponentModel.DataAnnotations;

namespace CodeRoom.Web.Models;

public class CourseModule
{
    public int Id { get; set; }

    public int CourseId { get; set; }

    [Required, StringLength(120, MinimumLength = 3)]
    public string Title { get; set; } = string.Empty;

    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [Range(1, 999)]
    public int Order { get; set; }

    public Course Course { get; set; } = null!;
    public ICollection<Lesson> Lessons { get; set; } = [];
}
