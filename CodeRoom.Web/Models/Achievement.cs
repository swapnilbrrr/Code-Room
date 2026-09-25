using System.ComponentModel.DataAnnotations;

namespace CodeRoom.Web.Models;

public class Achievement
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [Required, StringLength(40)]
    public string Code { get; set; } = string.Empty;

    [StringLength(20)]
    public string Icon { get; set; } = "★";

    public int XpReward { get; set; }

    public ICollection<UserAchievement> UserAchievements { get; set; } = [];
}
