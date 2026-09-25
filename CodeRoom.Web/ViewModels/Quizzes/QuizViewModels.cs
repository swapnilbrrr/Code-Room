using CodeRoom.Web.Models;

namespace CodeRoom.Web.ViewModels.Quizzes;

public class TakeQuizViewModel
{
    public Quiz Quiz { get; set; } = null!;
    public IReadOnlyList<Question> Questions { get; set; } = [];
}

public class QuizResultViewModel
{
    public string QuizTitle { get; set; } = string.Empty;
    public int CourseId { get; set; }
    public int Score { get; set; }
    public int Total { get; set; }
    public int Percent => Total == 0 ? 0 : (int)Math.Round(Score * 100.0 / Total);
}
