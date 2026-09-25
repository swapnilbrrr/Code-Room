using CodeRoom.Web.Data;
using CodeRoom.Web.Models;
using CodeRoom.Web.Services;
using CodeRoom.Web.ViewModels.Learning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CodeRoom.Web.Controllers;

[Authorize]
public class LessonsController(ApplicationDbContext db) : Controller
{
    // id = course id, optional lessonId selects the active lesson.
    public async Task<IActionResult> Index(int id, int? lessonId)
    {
        var course = await db.Courses
            .Include(c => c.Lessons.OrderBy(l => l.Order))
            .FirstOrDefaultAsync(c => c.Id == id);

        if (course is null || course.Lessons.Count == 0)
        {
            return NotFound();
        }

        var current = lessonId is null
            ? course.Lessons.First()
            : course.Lessons.FirstOrDefault(l => l.Id == lessonId) ?? course.Lessons.First();

        current.Content = LessonContentBuilder.EnsureRichContent(
            current.Content,
            course.Title,
            current.Title,
            current.Order);

        current.VideoUrl ??= current.Order == 1
            ? LessonMediaCatalog.VideoFor(course.Title)
            : null;

        current.ResourceUrl ??= current.Order == 1
            ? LessonMediaCatalog.ResourceFor(course.Title)
            : null;

        var userId = User.GetUserId();
        var completed = await db.Progress
            .Where(p => p.UserId == userId && p.IsCompleted && p.Lesson.CourseId == id)
            .Select(p => p.LessonId)
            .ToListAsync();

        var quiz = await db.Quizzes.FirstOrDefaultAsync(q => q.CourseId == id);
        var challenge = await db.Challenges
            .Where(ch => ch.CourseId == id && (ch.LessonId == current.Id || ch.LessonId == null))
            .OrderBy(ch => ch.LessonId == current.Id ? 0 : 1)
            .ThenBy(ch => ch.Id)
            .FirstOrDefaultAsync();

        var model = new LessonViewModel
        {
            Course = course,
            Current = current,
            Lessons = course.Lessons.ToList(),
            CompletedLessonIds = [.. completed],
            QuizId = quiz?.Id,
            ChallengeId = challenge?.Id
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(int lessonId, int courseId)
    {
        var lesson = await db.Lessons
            .Include(l => l.Course)
                .ThenInclude(c => c.Lessons)
            .FirstOrDefaultAsync(l => l.Id == lessonId);

        if (lesson is null)
        {
            return NotFound();
        }

        var userId = User.GetUserId();

        if (!await db.Enrollments.AnyAsync(e => e.UserId == userId && e.CourseId == lesson.CourseId))
        {
            db.Enrollments.Add(new Enrollment { UserId = userId, CourseId = lesson.CourseId });
        }

        var progress = await db.Progress
            .FirstOrDefaultAsync(p => p.UserId == userId && p.LessonId == lessonId);

        var wasAlreadyCompleted = progress?.IsCompleted == true;

        if (progress is null)
        {
            db.Progress.Add(new Progress
            {
                UserId = userId,
                LessonId = lessonId,
                IsCompleted = true,
                CompletedAt = DateTime.UtcNow
            });
        }
        else
        {
            progress.IsCompleted = true;
            progress.CompletedAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync();

        if (!wasAlreadyCompleted)
        {
            await LearningActivityService.RecordAsync(
                db,
                userId,
                "LessonCompleted",
                $"Completed {lesson.Title}",
                "Lesson completed",
                $"Nice work. {lesson.Title} is now marked complete.",
                $"/Lessons/Index/{lesson.CourseId}?lessonId={lesson.Id}",
                "LessonCompleted");

            var completedCount = await db.Progress.CountAsync(p =>
                p.UserId == userId &&
                p.IsCompleted &&
                p.Lesson.CourseId == lesson.CourseId);

            if (completedCount == lesson.Course.Lessons.Count)
            {
                var title = "Course completed";
                var alreadyNotified = await db.Notifications.AnyAsync(n =>
                    n.UserId == userId &&
                    n.Type == "CourseCompleted" &&
                    n.Title == title &&
                    n.LinkUrl == $"/Courses/Details/{lesson.CourseId}");

                if (!alreadyNotified)
                {
                    db.Notifications.Add(new Notification
                    {
                        UserId = userId,
                        Type = "CourseCompleted",
                        Title = title,
                        Message = $"You completed every lesson in {lesson.Course.Title}.",
                        LinkUrl = $"/Courses/Details/{lesson.CourseId}",
                        CreatedAt = DateTime.UtcNow
                    });

                    db.UserActivities.Add(new UserActivity
                    {
                        UserId = userId,
                        ActivityType = "CourseCompleted",
                        Description = $"Completed {lesson.Course.Title}",
                        CreatedAt = DateTime.UtcNow
                    });

                    await db.SaveChangesAsync();
                }
            }

            await LearningActivityService.TryRecordStreakMilestoneAsync(db, userId);

            TempData["ToastTitle"] = "Lesson completed";
            TempData["ToastMessage"] = "Progress saved. Keep your streak alive.";
            TempData["ToastIcon"] = "✓";
        }

        return RedirectToAction(nameof(Index), new { id = lesson.CourseId, lessonId });
    }
}
