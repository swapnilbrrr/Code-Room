using CodeRoom.Web.Data;
using CodeRoom.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CodeRoom.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = Roles.AdminArea)]
public class DashboardController(ApplicationDbContext db) : Controller
{
    public async Task<IActionResult> Index()
    {
        ViewBag.CourseCount = await db.Courses.CountAsync();
        ViewBag.LessonCount = await db.Lessons.CountAsync();
        ViewBag.QuizCount = await db.Quizzes.CountAsync();
        ViewBag.StudentCount = await db.Users.CountAsync(u => u.Role == Roles.Student);
        ViewBag.UserCount = await db.Users.CountAsync();
        ViewBag.EnrollmentCount = await db.Enrollments.CountAsync();
        ViewBag.AttemptCount = await db.QuizAttempts.CountAsync();
        return View();
    }
}
