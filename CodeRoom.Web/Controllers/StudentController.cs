using CodeRoom.Web.Data;
using CodeRoom.Web.Models;
using CodeRoom.Web.Services;
using CodeRoom.Web.ViewModels.Learning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CodeRoom.Web.Controllers;

[Authorize]
public class StudentController(ApplicationDbContext db) : Controller
{
    public async Task<IActionResult> Index()
    {
        var userId = User.GetUserId();

        var enrollments = await db.Enrollments
            .Where(e => e.UserId == userId)
            .Include(e => e.Course)
                .ThenInclude(c => c.Lessons)
            .ToListAsync();

        var completedByCourse = await db.Progress
            .Where(p => p.UserId == userId && p.IsCompleted)
            .Include(p => p.Lesson)
            .GroupBy(p => p.Lesson.CourseId)
            .Select(g => new { CourseId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.CourseId, x => x.Count);

        var enrolled = enrollments
            .Select(e => new EnrolledCourseViewModel
            {
                Course = e.Course,
                TotalLessons = e.Course.Lessons.Count,
                CompletedLessons = completedByCourse.GetValueOrDefault(e.CourseId, 0)
            })
            .ToList();

        var attempts = await db.QuizAttempts
            .Where(a => a.UserId == userId)
            .Include(a => a.Quiz)
            .OrderByDescending(a => a.AttemptedAt)
            .Take(5)
            .ToListAsync();

        var announcements = await db.Announcements
            .Where(a => a.IsPublished)
            .OrderByDescending(a => a.PublishedAt)
            .Take(3)
            .ToListAsync();

        return View(new StudentDashboardViewModel
        {
            StudentName = User.GetDisplayName(),
            Enrolments = enrolled,
            RecentAttempts = attempts,
            Announcements = announcements
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Enroll(int courseId)
    {
        var userId = User.GetUserId();
        var course = await db.Courses.FindAsync(courseId);

        if (course is null)
        {
            return NotFound();
        }

        if (!await db.Enrollments.AnyAsync(e => e.UserId == userId && e.CourseId == courseId))
        {
            db.Enrollments.Add(new Enrollment { UserId = userId, CourseId = courseId });
            await db.SaveChangesAsync();
        }

        return RedirectToAction("Index", "Lessons", new { id = courseId });
    }
}
