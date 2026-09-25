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
        ViewBag.ChallengeCount = await db.Challenges.CountAsync();
        ViewBag.CertificateCount = await db.Certificates.CountAsync();
        ViewBag.AdminCount = await db.Users.CountAsync(u => u.Role == Roles.Admin || u.Role == Roles.SuperAdmin);
        ViewBag.AuditCount = await db.AdminAuditLogs.CountAsync();
        ViewBag.RecentUsers = await db.Users.OrderByDescending(u => u.CreatedAt).Take(5).ToListAsync();
        ViewBag.RecentAudit = await db.AdminAuditLogs.Include(x => x.User).OrderByDescending(x => x.CreatedAt).Take(6).ToListAsync();
        return View();
    }
}
