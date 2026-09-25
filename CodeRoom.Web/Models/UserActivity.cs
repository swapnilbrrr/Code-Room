using System.ComponentModel.DataAnnotations;

namespace CodeRoom.Web.Models;

public class UserActivity
{
    public int Id { get; set; }

    public int UserId { get; set; }

    [Required, StringLength(40)]
    public string ActivityType { get; set; } = string.Empty;

    [Required, StringLength(220)]
    public string Description { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
}
