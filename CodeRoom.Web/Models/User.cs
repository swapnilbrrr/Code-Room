using System.ComponentModel.DataAnnotations;

namespace CodeRoom.Web.Models;

public class User
{
    public int Id { get; set; }

    [Required, StringLength(80, MinimumLength = 3)]
    public string FullName { get; set; } = string.Empty;

    [Required, StringLength(30, MinimumLength = 3)]
    [RegularExpression("^[a-zA-Z0-9._-]+$")]
    public string Username { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(120)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Bio { get; set; }

    [Required, StringLength(30)]
    public string Role { get; set; } = "Student";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Enrollment> Enrollments { get; set; } = [];
    public ICollection<Progress> Progress { get; set; } = [];
    public ICollection<QuizAttempt> QuizAttempts { get; set; } = [];
    public ICollection<UserActivity> Activities { get; set; } = [];
    public ICollection<Notification> Notifications { get; set; } = [];
}
