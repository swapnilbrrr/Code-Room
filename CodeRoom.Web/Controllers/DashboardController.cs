using CodeRoom.Web.Data;
using CodeRoom.Web.Services;
using CodeRoom.Web.ViewModels.Dashboard;
using CodeRoom.Web.ViewModels.Learning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CodeRoom.Web.Controllers;

[Authorize]
public class DashboardController(ApplicationDbContext db) : Controller
{
    public async Task<IActionResult> Index()
    {
        if (User.IsInRole(Roles.Admin) || User.IsInRole(Roles.SuperAdmin))
        {
            return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
        }

        var userId = User.GetUserId();
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId);

        if (user is null)
        {
            return Challenge();
        }

        var enrollments = await db.Enrollments
            .Where(e => e.UserId == userId)
            .Include(e => e.Course)
                .ThenInclude(c => c.Lessons)
            .OrderByDescending(e => e.EnrolledAt)
            .ToListAsync();

        var completedByCourse = await db.Progress
            .Where(p => p.UserId == userId && p.IsCompleted)
            .Include(p => p.Lesson)
            .GroupBy(p => p.Lesson.CourseId)
            .Select(g => new { CourseId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.CourseId, x => x.Count);

        var courses = enrollments
            .Select(e => new EnrolledCourseViewModel
            {
                Course = e.Course,
                TotalLessons = e.Course.Lessons.Count,
                CompletedLessons = completedByCourse.GetValueOrDefault(e.CourseId, 0)
            })
            .ToList();

        var enrolledIds = enrollments.Select(e => e.CourseId).ToHashSet();
        var recommendationQuery = db.Courses
            .Where(c => c.IsPublished && !enrolledIds.Contains(c.Id))
            .OrderByDescending(c => c.IsCertification)
            .ThenBy(c => c.Category)
            .ThenByDescending(c => c.CreatedAt)
            .Take(6);

        var recommended = await recommendationQuery.ToListAsync();
        var primaryCategory = enrollments.Select(e => e.Course.Category).FirstOrDefault();

        if (!string.IsNullOrWhiteSpace(primaryCategory))
        {
            var sameCategory = recommended.Where(c => c.Category == primaryCategory).ToList();
            var otherCategory = recommended.Where(c => c.Category != primaryCategory).ToList();
            recommended = sameCategory.Concat(otherCategory).Take(4).ToList();
        }

        var allAttempts = await db.QuizAttempts
            .Where(a => a.UserId == userId)
            .Include(a => a.Quiz)
            .OrderByDescending(a => a.AttemptedAt)
            .ToListAsync();

        var activities = await db.UserActivities
            .Where(a => a.UserId == userId && a.CreatedAt >= DateTime.UtcNow.Date.AddDays(-364))
            .OrderBy(a => a.CreatedAt)
            .ToListAsync();

        var activityDays = BuildActivityDays(activities);
        var totalLessons = courses.Sum(c => c.TotalLessons);
        var completedLessons = courses.Sum(c => c.CompletedLessons);

        return View(new PersonalDashboardViewModel
        {
            FullName = user.FullName,
            Username = user.Username,
            RoleLabel = user.Role,
            IsAdmin = user.Role is Roles.Admin or Roles.SuperAdmin,
            Courses = courses.Take(4).ToList(),
            RecentAttempts = allAttempts.Take(5).ToList(),
            Announcements = await db.Announcements
                .Where(a => a.IsPublished)
                .OrderByDescending(a => a.PublishedAt)
                .Take(3)
                .ToListAsync(),
            RecentActivity = activities
                .OrderByDescending(a => a.CreatedAt)
                .Take(7)
                .Select(a => new RecentActivityViewModel
                {
                    ActivityType = a.ActivityType,
                    Description = a.Description,
                    CreatedAt = a.CreatedAt
                })
                .ToList(),
            ActivityDays = activityDays,
            CoursesEnrolled = courses.Count,
            LessonsCompleted = completedLessons,
            QuizAttempts = allAttempts.Count,
            ProgressPercent = totalLessons == 0 ? 0 : (int)Math.Round(completedLessons * 100.0 / totalLessons),
            QuizAverage = allAttempts.Count == 0
                ? 0
                : (int)Math.Round(allAttempts.Average(a => a.TotalQuestions == 0 ? 0 : a.Score * 100.0 / a.TotalQuestions)),
            LearningStreak = LearningActivityService.CalculateStreak(activities),
            Xp = user.Xp,
            Level = LearningActivityService.GetLevel(user.Xp),
            LevelProgress = LearningActivityService.GetLevelProgress(user.Xp),
            CertificateCount = await db.Certificates.CountAsync(c => c.UserId == userId),
            RecommendedCourses = recommended.Select(course => new RecommendedCourseViewModel
            {
                Course = course,
                Reason = !string.IsNullOrWhiteSpace(primaryCategory) && course.Category == primaryCategory
                    ? $"Because you're learning {primaryCategory}"
                    : "Recommended for your learning path"
            }).ToList()
        });
    }

    private static List<ActivityDayViewModel> BuildActivityDays(IEnumerable<Models.UserActivity> activities)
    {
        var counts = activities
            .GroupBy(a => a.CreatedAt.Date)
            .ToDictionary(g => g.Key, g => g.Count());

        var start = DateTime.UtcNow.Date.AddDays(-364);
        return Enumerable.Range(0, 365)
            .Select(offset =>
            {
                var date = start.AddDays(offset);
                var count = counts.GetValueOrDefault(date, 0);
                var level = count switch
                {
                    0 => 0,
                    1 => 1,
                    <= 3 => 2,
                    <= 6 => 3,
                    _ => 4
                };

                return new ActivityDayViewModel { Date = date, Count = count, Level = level };
            })
            .ToList();
    }
}
