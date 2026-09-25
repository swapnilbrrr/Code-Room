namespace CodeRoom.Web.Models;

public class QuizAttempt
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int QuizId { get; set; }
    public int Score { get; set; }
    public int TotalQuestions { get; set; }
    public DateTime AttemptedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public Quiz Quiz { get; set; } = null!;
}
