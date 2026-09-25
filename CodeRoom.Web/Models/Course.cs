using System.ComponentModel.DataAnnotations;

namespace CodeRoom.Web.Models;

public class Course
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Title is required.")]
    [StringLength(120, MinimumLength = 3)]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Slug is required.")]
    [StringLength(140)]
    [RegularExpression("^[a-z0-9-]+$", ErrorMessage = "Slug may only contain lowercase letters, numbers and hyphens.")]
    public string Slug { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required.")]
    [StringLength(1000, MinimumLength = 10)]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Category is required.")]
    [StringLength(60)]
    public string Category { get; set; } = string.Empty;

    [Required]
    [StringLength(30)]
    public string Level { get; set; } = "Beginner";

    [StringLength(300)]
    [Display(Name = "Thumbnail URL")]
    public string? ThumbnailUrl { get; set; }

    [Display(Name = "Published")]
    public bool IsPublished { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Lesson> Lessons { get; set; } = [];
    public ICollection<Enrollment> Enrollments { get; set; } = [];
}
