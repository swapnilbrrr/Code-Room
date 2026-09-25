using System.ComponentModel.DataAnnotations;

namespace CodeRoom.Web.Models;

public class Quiz
{
    public int Id { get; set; }

    [Display(Name = "Course")]
    public int CourseId { get; set; }

    [Required(ErrorMessage = "Title is required.")]
    [StringLength(150, MinimumLength = 3)]
    public string Title { get; set; } = string.Empty;

    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [Required, StringLength(30)]
    public string AssessmentType { get; set; } = "Quiz";

    [Range(0, 180)]
    public int TimeLimitMinutes { get; set; }

    [Range(0, 100)]
    public int PassingScorePercent { get; set; } = 70;

    public bool IsCertificationExam { get; set; }

    public Course Course { get; set; } = null!;
    public ICollection<Question> Questions { get; set; } = [];
    public ICollection<QuizAttempt> Attempts { get; set; } = [];
}
