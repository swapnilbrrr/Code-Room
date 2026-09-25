using System.ComponentModel.DataAnnotations;

namespace CodeRoom.Web.ViewModels.Admin;

public class ChallengeFormViewModel
{
    public int Id { get; set; }

    [Required]
    public int CourseId { get; set; }

    public int? LessonId { get; set; }

    [Required, StringLength(160, MinimumLength = 3)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(1800, MinimumLength = 10)]
    public string Instructions { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? StarterCode { get; set; }

    [StringLength(600)]
    public string? Hint { get; set; }

    [Required, StringLength(1000)]
    public string ExpectedAnswer { get; set; } = string.Empty;

    [Required]
    public string ValidationMode { get; set; } = "Exact";

    [Range(5, 500)]
    public int Points { get; set; } = 50;

    public bool IsEdit { get; set; }
}
