using CodeRoom.Web.Data;
using CodeRoom.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CodeRoom.Web.Controllers;

public class CoursesController(ApplicationDbContext db) : Controller
{
    public async Task<IActionResult> Index()
    {
        var courses = await db.Courses
            .Where(c => c.IsPublished)
            .Include(c => c.Lessons)
            .OrderBy(c => c.Title)
            .ToListAsync();

        return View(courses);
    }

    public async Task<IActionResult> Details(int id)
    {
        var course = await db.Courses
            .Include(c => c.Lessons.OrderBy(l => l.Order))
            .FirstOrDefaultAsync(c => c.Id == id);

        if (course is null)
        {
            return NotFound();
        }

        var quiz = await db.Quizzes.FirstOrDefaultAsync(q => q.CourseId == id);
        ViewData["QuizId"] = quiz?.Id;

        return View(course);
    }
}
