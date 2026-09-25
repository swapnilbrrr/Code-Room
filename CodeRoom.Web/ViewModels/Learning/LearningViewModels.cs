using CodeRoom.Web.Models;

namespace CodeRoom.Web.ViewModels.Learning;

public class LessonViewModel
{
    public Course Course { get; set; } = null!;
    public Lesson Current { get; set; } = null!;
    public IReadOnlyList<Lesson> Lessons { get; set; } = [];
    public HashSet<int> CompletedLessonIds { get; set; } = [];
    public int? QuizId { get; set; }

    public int CompletedCount => CompletedLessonIds.Count;
    public int TotalCount => Lessons.Count;
    public int ProgressPercent => TotalCount == 0 ? 0 : (int)Math.Round(CompletedCount * 100.0 / TotalCount);
    public bool IsCurrentCompleted => CompletedLessonIds.Contains(Current.Id);
}

public class EnrolledCourseViewModel
{
    public Course Course { get; set; } = null!;
    public int CompletedLessons { get; set; }
    public int TotalLessons { get; set; }
    public int ProgressPercent => TotalLessons == 0 ? 0 : (int)Math.Round(CompletedLessons * 100.0 / TotalLessons);
}

public class StudentDashboardViewModel
{
    public string StudentName { get; set; } = "Learner";
    public IReadOnlyList<EnrolledCourseViewModel> Enrolments { get; set; } = [];
    public IReadOnlyList<QuizAttempt> RecentAttempts { get; set; } = [];
    public IReadOnlyList<Announcement> Announcements { get; set; } = [];

    public int EnrolmentCount => Enrolments.Count;
    public int LessonsCompleted => Enrolments.Sum(e => e.CompletedLessons);
    public int QuizAverage => RecentAttempts.Count == 0
        ? 0
        : (int)Math.Round(RecentAttempts.Average(a => a.TotalQuestions == 0 ? 0 : a.Score * 100.0 / a.TotalQuestions));
}
