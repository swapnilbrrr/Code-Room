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
            .Include(c => c.Modules.OrderBy(m => m.Order))
                .ThenInclude(m => m.Lessons.OrderBy(l => l.Order))
            .Include(c => c.Lessons.OrderBy(l => l.Order))
            .Include(c => c.Challenges.OrderBy(ch => ch.Id))
            .FirstOrDefaultAsync(c => c.Id == id);

        if (course is null)
        {
            return NotFound();
        }

        var quiz = await db.Quizzes.FirstOrDefaultAsync(q => q.CourseId == id);
        var resources = await db.Resources
            .Where(r => r.CourseId == id)
            .OrderBy(r => r.Title)
            .Take(8)
            .ToListAsync();
        ViewData["QuizId"] = quiz?.Id;
        ViewData["CourseResources"] = resources;

        return View(course);
    }
}
