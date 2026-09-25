using System.ComponentModel.DataAnnotations;

namespace CodeRoom.Web.Models;

public class Lesson
{
    public int Id { get; set; }

    [Display(Name = "Course")]
    public int CourseId { get; set; }

    public int? CourseModuleId { get; set; }

    [Required(ErrorMessage = "Title is required.")]
    [StringLength(150, MinimumLength = 3)]
    public string Title { get; set; } = string.Empty;

    [StringLength(180)]
    public string? Summary { get; set; }

    [Required(ErrorMessage = "Content is required.")]
    [DataType(DataType.MultilineText)]
    public string Content { get; set; } = string.Empty;

    [Required, StringLength(30)]
    public string ContentType { get; set; } = "Reading";

    [StringLength(300)]
    [Display(Name = "Video URL")]
    public string? VideoUrl { get; set; }

    [StringLength(300)]
    [Display(Name = "Audio URL")]
    public string? AudioUrl { get; set; }

    [StringLength(300)]
    [Display(Name = "Resource URL")]
    public string? ResourceUrl { get; set; }

    [Range(1, 999, ErrorMessage = "Order must be a positive number.")]
    public int Order { get; set; }

    [Range(1, 600)]
    [Display(Name = "Duration (minutes)")]
    public int DurationMinutes { get; set; } = 10;

    [Display(Name = "Published")]
    public bool IsPublished { get; set; } = true;

    public Course Course { get; set; } = null!;
    public CourseModule? CourseModule { get; set; }
    public ICollection<Challenge> Challenges { get; set; } = [];
}
