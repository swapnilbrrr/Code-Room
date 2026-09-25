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

    [Range(10, 1000)]
    public int EstimatedMinutes { get; set; } = 120;

    [Display(Name = "Certification course")]
    public bool IsCertification { get; set; }

    [StringLength(160)]
    public string? CertificateName { get; set; }

    [Range(50, 100)]
    public int PassingScorePercent { get; set; } = 70;

    [StringLength(300)]
    [Display(Name = "Thumbnail URL")]
    public string? ThumbnailUrl { get; set; }

    [Display(Name = "Published")]
    public bool IsPublished { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<CourseModule> Modules { get; set; } = [];
    public ICollection<Lesson> Lessons { get; set; } = [];
    public ICollection<Enrollment> Enrollments { get; set; } = [];
    public ICollection<Challenge> Challenges { get; set; } = [];
    public ICollection<Certificate> Certificates { get; set; } = [];
}
