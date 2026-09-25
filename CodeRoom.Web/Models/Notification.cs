using System.ComponentModel.DataAnnotations;

namespace CodeRoom.Web.Models;

public class Notification
{
    public int Id { get; set; }

    public int UserId { get; set; }

    [Required, StringLength(40)]
    public string Type { get; set; } = string.Empty;

    [Required, StringLength(150)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(500)]
    public string Message { get; set; } = string.Empty;

    [StringLength(300)]
    public string? LinkUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsRead { get; set; }

    public User User { get; set; } = null!;
}
