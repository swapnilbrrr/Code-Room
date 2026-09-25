using CodeRoom.Web.Models;

namespace CodeRoom.Web.ViewModels.Dashboard;

public class ActivityDayViewModel
{
    public DateTime Date { get; set; }
    public int Count { get; set; }
    public int Level { get; set; }
}

public class RecentActivityViewModel
{
    public string ActivityType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class PersonalDashboardViewModel
{
    public string FullName { get; set; } = "Learner";
    public string Username { get; set; } = "learner";
    public string RoleLabel { get; set; } = "Student";
    public bool IsAdmin { get; set; }

    public IReadOnlyList<EnrolledCourseViewModel> Courses { get; set; } = [];
    public IReadOnlyList<QuizAttempt> RecentAttempts { get; set; } = [];
    public IReadOnlyList<Announcement> Announcements { get; set; } = [];
    public IReadOnlyList<RecentActivityViewModel> RecentActivity { get; set; } = [];
    public IReadOnlyList<ActivityDayViewModel> ActivityDays { get; set; } = [];

    public int CoursesEnrolled { get; set; }
    public int LessonsCompleted { get; set; }
    public int QuizAttempts { get; set; }
    public int ProgressPercent { get; set; }
    public int QuizAverage { get; set; }
    public int LearningStreak { get; set; }
}
