using System.ComponentModel.DataAnnotations;

namespace CodeRoom.Web.Models;

public class AdminAuditLog
{
    public int Id { get; set; }
    public int UserId { get; set; }

    [Required, StringLength(60)]
    public string Action { get; set; } = string.Empty;

    [Required, StringLength(80)]
    public string EntityType { get; set; } = string.Empty;

    [StringLength(120)]
    public string? EntityName { get; set; }

    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
}
