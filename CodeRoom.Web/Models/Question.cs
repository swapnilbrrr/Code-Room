using System.ComponentModel.DataAnnotations;

namespace CodeRoom.Web.Models;

public class Question
{
    public int Id { get; set; }

    [Display(Name = "Quiz")]
    public int QuizId { get; set; }

    [Required(ErrorMessage = "Question text is required.")]
    [StringLength(400)]
    [Display(Name = "Question")]
    public string QuestionText { get; set; } = string.Empty;

    [Required(ErrorMessage = "Option A is required.")]
    [StringLength(200)]
    public string OptionA { get; set; } = string.Empty;

    [Required(ErrorMessage = "Option B is required.")]
    [StringLength(200)]
    public string OptionB { get; set; } = string.Empty;

    [Required(ErrorMessage = "Option C is required.")]
    [StringLength(200)]
    public string OptionC { get; set; } = string.Empty;

    [Required(ErrorMessage = "Option D is required.")]
    [StringLength(200)]
    public string OptionD { get; set; } = string.Empty;

    [Required]
    [RegularExpression("^[ABCD]$", ErrorMessage = "Correct option must be A, B, C or D.")]
    [Display(Name = "Correct option")]
    public string CorrectOption { get; set; } = "A";

    public Quiz Quiz { get; set; } = null!;
}
