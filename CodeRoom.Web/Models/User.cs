namespace CodeRoom.Web.Models;

public class User
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "Student";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Enrollment> Enrollments { get; set; } = [];
    public ICollection<Progress> Progress { get; set; } = [];
    public ICollection<QuizAttempt> QuizAttempts { get; set; } = [];
}
