using CodeRoom.Web.Models;
using System.ComponentModel.DataAnnotations;
using CodeRoom.Web.ViewModels.Dashboard;

namespace CodeRoom.Web.ViewModels.Profile;

public class ProfileViewModel
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public string RoleLabel { get; set; } = "Student";
    public bool IsAdmin { get; set; }

    public int CoursesEnrolled { get; set; }
    public int LessonsCompleted { get; set; }
    public int QuizAttempts { get; set; }
    public int ProgressPercent { get; set; }
    public int LearningStreak { get; set; }
    public int Xp { get; set; }
    public int Level { get; set; }
    public int LevelProgress { get; set; }
    public int CertificateCount { get; set; }
    public string? AvatarUrl { get; set; }
    public IReadOnlyList<Achievement> Achievements { get; set; } = [];

    public IReadOnlyList<ActivityDayViewModel> ActivityDays { get; set; } = [];
    public IReadOnlyList<RecentActivityViewModel> RecentActivity { get; set; } = [];
}

public class ProfileSettingsViewModel
{
    [Required, StringLength(80, MinimumLength = 3, ErrorMessage = "Full name must be between 3 and 80 characters.")]
    [Display(Name = "Full name")]
    public string FullName { get; set; } = string.Empty;

    [Required, StringLength(30, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 30 characters.")]
    [RegularExpression("^[a-zA-Z0-9._-]+$", ErrorMessage = "Username may contain letters, numbers, dots, underscores and hyphens.")]
    public string Username { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(120)]
    public string Email { get; set; } = string.Empty;

    [StringLength(500)]
    [Display(Name = "Bio")]
    public string? Bio { get; set; }

    [StringLength(300)]
    [Url(ErrorMessage = "Enter a valid avatar URL.")]
    [Display(Name = "Avatar URL")]
    public string? AvatarUrl { get; set; }

    [StringLength(20)]
    [Display(Name = "Theme")]
    public string ThemePreference { get; set; } = "system";

    [StringLength(20)]
    [Display(Name = "Profile visibility")]
    public string ProfileVisibility { get; set; } = "Public";

    [Display(Name = "Email notifications")]
    public bool EmailNotificationsEnabled { get; set; } = true;

    [DataType(DataType.Password)]
    [Display(Name = "Current password")]
    public string? CurrentPassword { get; set; }

    [StringLength(100, MinimumLength = 8, ErrorMessage = "New password must be at least 8 characters.")]
    [DataType(DataType.Password)]
    [Display(Name = "New password")]
    public string? NewPassword { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Confirm new password")]
    public string? ConfirmNewPassword { get; set; }
}
