using System.ComponentModel.DataAnnotations;

namespace CodeRoom.Web.Models;

public class Certificate
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int CourseId { get; set; }
    public int QuizAttemptId { get; set; }

    [Required, StringLength(40)]
    public string CertificateNumber { get; set; } = string.Empty;

    [Required, StringLength(160)]
    public string Title { get; set; } = string.Empty;

    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public Course Course { get; set; } = null!;
    public QuizAttempt QuizAttempt { get; set; } = null!;
}
