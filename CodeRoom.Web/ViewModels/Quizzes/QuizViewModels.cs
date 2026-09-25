using CodeRoom.Web.Models;

namespace CodeRoom.Web.ViewModels.Quizzes;

public class TakeQuizViewModel
{
    public Quiz Quiz { get; set; } = null!;
    public IReadOnlyList<Question> Questions { get; set; } = [];
    public int TimeLimitMinutes => Quiz.TimeLimitMinutes;
    public bool IsExam => !string.Equals(Quiz.AssessmentType, "Quiz", StringComparison.OrdinalIgnoreCase);
}

public class QuizResultViewModel
{
    public string QuizTitle { get; set; } = string.Empty;
    public int CourseId { get; set; }
    public int Score { get; set; }
    public int Total { get; set; }
    public int Percent => Total == 0 ? 0 : (int)Math.Round(Score * 100.0 / Total);
    public string AssessmentType { get; set; } = "Quiz";
    public int PassingScorePercent { get; set; } = 70;
    public bool IsCertificationExam { get; set; }
    public Certificate? Certificate { get; set; }
    public bool Passed => IsCertificationExam || !string.Equals(AssessmentType, "Quiz", StringComparison.OrdinalIgnoreCase)
        ? Percent >= PassingScorePercent
        : true;
}
