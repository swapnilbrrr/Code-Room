using CodeRoom.Web.Data;
using CodeRoom.Web.Models;
using CodeRoom.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CodeRoom.Web.Controllers;

[Authorize]
public class StudentController(ApplicationDbContext db) : Controller
{
    public IActionResult Index() => RedirectToAction("Index", "Dashboard");

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

        var alreadyEnrolled = await db.Enrollments.AnyAsync(e => e.UserId == userId && e.CourseId == courseId);
        if (!alreadyEnrolled)
        {
            db.Enrollments.Add(new Enrollment { UserId = userId, CourseId = courseId });
            await db.SaveChangesAsync();

            await LearningActivityService.RecordAsync(
                db,
                userId,
                "CourseEnrolled",
                $"Enrolled in {course.Title}",
                "Course enrolled",
                $"You are now enrolled in {course.Title}. Your learning journey starts here.",
                $"/Lessons/Index/{course.Id}",
                "CourseEnrollment");

            var totalEnrollments = await db.Enrollments.CountAsync(e => e.UserId == userId);
            if (totalEnrollments == 1)
            {
                await LearningActivityService.RecordAsync(
                    db,
                    userId,
                    "AchievementUnlocked",
                    "Unlocked the First Course achievement",
                    "Achievement unlocked",
                    "You enrolled in your first Code-Room course. Nice start!",
                    "/Profile",
                    "AchievementUnlocked");
            }

            await LearningActivityService.TryRecordStreakMilestoneAsync(db, userId);

            TempData["ToastTitle"] = "Course enrolled";
            TempData["ToastMessage"] = $"You're ready to start {course.Title}.";
            TempData["ToastIcon"] = "✓";
        }

        return RedirectToAction("Index", "Lessons", new { id = courseId });
    }
}
