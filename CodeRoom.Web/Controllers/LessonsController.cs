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

        var userId = User.GetUserId();
        var completed = await db.Progress
            .Where(p => p.UserId == userId && p.IsCompleted && p.Lesson.CourseId == id)
            .Select(p => p.LessonId)
            .ToListAsync();

        var quiz = await db.Quizzes.FirstOrDefaultAsync(q => q.CourseId == id);

        var model = new LessonViewModel
        {
            Course = course,
            Current = current,
            Lessons = course.Lessons.ToList(),
            CompletedLessonIds = [.. completed],
            QuizId = quiz?.Id
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(int lessonId, int courseId)
    {
        var lesson = await db.Lessons.FindAsync(lessonId);
        if (lesson is null)
        {
            return NotFound();
        }

        var userId = User.GetUserId();

        // Ensure the student is enrolled so progress is meaningful.
        if (!await db.Enrollments.AnyAsync(e => e.UserId == userId && e.CourseId == lesson.CourseId))
        {
            db.Enrollments.Add(new Enrollment { UserId = userId, CourseId = lesson.CourseId });
        }

        var progress = await db.Progress
            .FirstOrDefaultAsync(p => p.UserId == userId && p.LessonId == lessonId);

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
        return RedirectToAction(nameof(Index), new { id = lesson.CourseId, lessonId });
    }
}
